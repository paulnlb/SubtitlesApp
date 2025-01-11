using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Core.Services;
using SubtitlesServer.Application.Interfaces;
using SubtitlesServer.Infrastructure.Configs;

namespace SubtitlesServer.Infrastructure.Services;

public class LlmTranslationService : ITranslationService
{
    private readonly OllamaConfig _config;
    private readonly LanguageService _languageService;
    private readonly ILlmService _llmService;
    private readonly ILogger<LlmTranslationService> _logger;
    private readonly IMapper _mapper;

    public LlmTranslationService(
        IOptions<OllamaConfig> ollamaOptions,
        LanguageService languageService,
        ILlmService llmService,
        ILogger<LlmTranslationService> logger,
        IMapper mapper
    )
    {
        _config = ollamaOptions.Value;
        _languageService = languageService;
        _llmService = llmService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ListResult<SubtitleDto>> TranslateAsync(TranslationRequestDto requestDto)
    {
        var targetLanguage = _languageService.GetLanguageByCode(requestDto.TargetLanguageCode);

        if (targetLanguage == null)
        {
            var error = new Error(
                ErrorCode.BadRequest,
                $"Target language code {requestDto.TargetLanguageCode} is invalid or not supported"
            );
            return ListResult<SubtitleDto>.Failure(error);
        }

        if (requestDto.SourceSubtitles == null || requestDto.SourceSubtitles.Count == 0)
        {
            var error = new Error(ErrorCode.BadRequest, "Provide at least one subtitle to translate");
            return ListResult<SubtitleDto>.Failure(error);
        }

        var systemPrompt = string.Format(_config.DefaultSystemPrompt, targetLanguage.Name);
        var chatHistory = new List<LlmMessageDto>() { new("system", systemPrompt) };
        var userPrompt = SerializeSubtitlesToPrompt(requestDto.SourceSubtitles);

        var llmResult = await _llmService.SendAsync(chatHistory, userPrompt);

        if (llmResult.IsFailure)
        {
            return ListResult<SubtitleDto>.Failure(llmResult.Error);
        }

        return DeserializeSubtitles(requestDto, llmResult.Value);
    }

    public AsyncEnumerableResult<SubtitleDto> TranslateAndStreamAsync(TranslationRequestDto requestDto)
    {
        var targetLanguage = _languageService.GetLanguageByCode(requestDto.TargetLanguageCode);

        if (targetLanguage == null)
        {
            var error = new Error(
                ErrorCode.BadRequest,
                $"Target language code {requestDto.TargetLanguageCode} is invalid or not supported"
            );
            return AsyncEnumerableResult<SubtitleDto>.Failure(error);
        }

        if (requestDto.SourceSubtitles == null || requestDto.SourceSubtitles.Count == 0)
        {
            var error = new Error(ErrorCode.BadRequest, "Provide at least one subtitle to translate");
            return AsyncEnumerableResult<SubtitleDto>.Failure(error);
        }

        return AsyncEnumerableResult<SubtitleDto>.Success(StreamTranslationAsync(targetLanguage, requestDto));
    }

    private async IAsyncEnumerable<SubtitleDto> StreamTranslationAsync(
        Language targetLanguage,
        TranslationRequestDto requestDto
    )
    {
        var systemPrompt = string.Format(_config.SingleSubtitleSystemPrompt, targetLanguage.Name);
        var chatHistory = new List<LlmMessageDto>() { new("system", systemPrompt) };
        var serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic, UnicodeRanges.Arabic),
        };

        var currentIndex = 0;
        const int RetryLimit = 3;
        var retryCounter = 0;

        while (currentIndex < requestDto.SourceSubtitles.Count && retryCounter < RetryLimit)
        {
            var subtitleDto = requestDto.SourceSubtitles[currentIndex];
            var serializedPrompt = JsonSerializer
                .Serialize(_mapper.Map<Translation>(subtitleDto), serializerOptions)
                .Replace("\\u0027", "\'"); // fix System.Test.Json serializing a single quote into \u0027

            var llmResult = await _llmService.SendAsync(chatHistory, serializedPrompt);

            if (llmResult.IsFailure)
            {
                _logger.LogError("Llm service error: {error}", llmResult.Error.Description);
                retryCounter++;
                continue;
            }

            var translationResult = DeserializeTranslation(llmResult.Value, serializerOptions);

            if (translationResult.IsFailure)
            {
                _logger.LogError(
                    "Deserialization of translated subtitle failed: {error}",
                    translationResult.Error.Description
                );
                retryCounter++;
                continue;
            }

            if (!ValidateTranslatedSubtitle(requestDto.TargetLanguageCode, subtitleDto, translationResult.Value))
            {
                _logger.LogError(
                    "Translated subtitle does not match the source subtitle. "
                        + "Source subtitle: {src}. Translated subtitle: {trs}",
                    subtitleDto,
                    translationResult.Value
                );
                retryCounter++;
                continue;
            }

            chatHistory.Add(new LlmMessageDto("user", serializedPrompt));
            chatHistory.Add(new LlmMessageDto("assistant", llmResult.Value));

            currentIndex++;
            retryCounter = 0;

            var translatedSubtitle = _mapper.Map<SubtitleDto>(subtitleDto);
            _mapper.Map(translationResult.Value, translatedSubtitle);

            yield return translatedSubtitle;
        }
    }

    private string SerializeSubtitlesToPrompt(List<SubtitleDto> subtitlesDtos)
    {
        var serializedSubtitles = JsonSerializer.Serialize(
            _mapper.Map<List<Translation>>(subtitlesDtos),
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic, UnicodeRanges.Arabic),
            }
        );

        var rawPrompt =
            @"Translate the following subtitles, following the instructions above:
{0}";
        return string.Format(rawPrompt, serializedSubtitles);
    }

    private ListResult<SubtitleDto> DeserializeSubtitles(TranslationRequestDto requestDto, string llmResponse)
    {
        var llmTranslations = JsonSerializer.Deserialize<List<Translation>>(
            llmResponse,
            new JsonSerializerOptions { WriteIndented = true }
        );

        if (llmTranslations?.Count != requestDto.SourceSubtitles.Count)
        {
            var error = new Error(
                ErrorCode.BadGateway,
                $"Sizes of source and translated subtitles lists do not match. Expected: {requestDto.SourceSubtitles.Count}, but was: {llmTranslations?.Count}"
            );
            return ListResult<SubtitleDto>.Failure(error);
        }

        if (!ValidateTranslations(requestDto, llmTranslations))
        {
            var error = new Error(ErrorCode.BadGateway, $"Translated and original subtitles do not match.");
            return ListResult<SubtitleDto>.Failure(error);
        }

        var translatedSubtitlesDtos = _mapper.Map<List<SubtitleDto>>(requestDto.SourceSubtitles);
        _mapper.Map(llmTranslations, translatedSubtitlesDtos);

        return ListResult<SubtitleDto>.Success(translatedSubtitlesDtos);
    }

    private static Result<Translation> DeserializeTranslation(string llmResponse, JsonSerializerOptions serializerOptions)
    {
        try
        {
            var translation = JsonSerializer.Deserialize<Translation>(llmResponse, serializerOptions);

            if (translation == null)
            {
                var error = new Error(ErrorCode.BadGateway, "Deserialized subtitlte is empty");
                return Result<Translation>.Failure(error);
            }

            return Result<Translation>.Success(translation);
        }
        catch (Exception e)
        {
            var error = new Error(ErrorCode.BadGateway, e.Message);
            return Result<Translation>.Failure(error);
        }
    }

    private static bool ValidateTranslations(TranslationRequestDto requestDto, List<Translation> translations)
    {
        for (int i = 0; i < requestDto.SourceSubtitles.Count; i++)
        {
            var sourceSubtitle = requestDto.SourceSubtitles[i];
            var llmSubtitle = translations[i];

            if (!ValidateTranslatedSubtitle(requestDto.TargetLanguageCode, sourceSubtitle, llmSubtitle))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateTranslatedSubtitle(
        string targetLanguageCode,
        SubtitleDto subtitleDto,
        Translation translation
    )
    {
        return targetLanguageCode == translation.LanguageCode;
    }
}
