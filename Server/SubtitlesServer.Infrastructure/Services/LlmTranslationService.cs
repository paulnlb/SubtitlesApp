using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
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

    public LlmTranslationService(
        IOptions<OllamaConfig> ollamaOptions,
        LanguageService languageService,
        ILlmService llmService,
        ILogger<LlmTranslationService> logger
    )
    {
        _config = ollamaOptions.Value;
        _languageService = languageService;
        _llmService = llmService;
        _logger = logger;
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
            var error = new Error(
                ErrorCode.BadRequest,
                "Provide at least one subtitle to translate"
            );
            return ListResult<SubtitleDto>.Failure(error);
        }

        var systemPrompt = string.Format(_config.DefaultSystemPrompt, targetLanguage.EnglishName);
        var chatHistory = new List<LlmMessageDto>() { new("system", systemPrompt) };
        var userPrompt = SerializeSubtitlesToPrompt(requestDto.SourceSubtitles);

        var llmResult = await _llmService.SendAsync(chatHistory, userPrompt);
        if (llmResult.IsFailure)
        {
            return ListResult<SubtitleDto>.Failure(llmResult.Error);
        }

        return DeserializeSubtitles(requestDto, llmResult.Value);
    }

    public AsyncEnumerableResult<SubtitleDto> TranslateAndStreamAsync(
        TranslationRequestDto requestDto
    )
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
            var error = new Error(
                ErrorCode.BadRequest,
                "Provide at least one subtitle to translate"
            );
            return AsyncEnumerableResult<SubtitleDto>.Failure(error);
        }

        return AsyncEnumerableResult<SubtitleDto>.Success(
            StreamTranslationAsync(targetLanguage, requestDto)
        );
    }

    private async IAsyncEnumerable<SubtitleDto> StreamTranslationAsync(
        Language targetLanguage,
        TranslationRequestDto requestDto
    )
    {
        var systemPrompt = string.Format(
            _config.SingleSubtitleSystemPrompt,
            targetLanguage.EnglishName
        );
        var chatHistory = new List<LlmMessageDto>() { new("system", systemPrompt) };
        var serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(
                UnicodeRanges.BasicLatin,
                UnicodeRanges.Cyrillic,
                UnicodeRanges.Arabic
            ),
        };

        var currentIndex = 0;
        const int retryLimit = 3;
        var retryCounter = 0;

        while (currentIndex < requestDto.SourceSubtitles.Count && retryCounter < retryLimit)
        {
            var subtitleDto = requestDto.SourceSubtitles[currentIndex];

            var serializedSubtitle = JsonSerializer
                .Serialize(subtitleDto, serializerOptions)
                .Replace("\\u0027", "\'"); // fix System.Test.Json serializing a single quote into \u0027

            var llmResult = await _llmService.SendAsync(chatHistory, serializedSubtitle);

            if (llmResult.IsFailure)
            {
                _logger.LogError("Llm service error: {error}", llmResult.Error.Description);
                retryCounter++;
                continue;
            }

            var translatedSubtitleResult = DeserializeSubtitle(llmResult.Value, serializerOptions);

            if (translatedSubtitleResult.IsFailure)
            {
                _logger.LogError(
                    "Deserialization of translated subtitle failed: {error}",
                    translatedSubtitleResult.Error.Description
                );
                retryCounter++;
                continue;
            }

            if (
                !ValidateTranslatedSubtitle(
                    requestDto.TargetLanguageCode,
                    subtitleDto,
                    translatedSubtitleResult.Value
                )
            )
            {
                _logger.LogError(
                    "Translated subtitle does not match the source subtitle. "
                        + "Source subtitle: {src}. Translated subtitle: {trs}",
                    subtitleDto,
                    translatedSubtitleResult.Value
                );
                retryCounter++;
                continue;
            }

            chatHistory.Add(new LlmMessageDto("user", serializedSubtitle));
            chatHistory.Add(new LlmMessageDto("assistant", llmResult.Value));

            currentIndex++;
            retryCounter = 0;

            yield return translatedSubtitleResult.Value;
        }
    }

    private static string SerializeSubtitlesToPrompt(List<SubtitleDto> subtitlesDtos)
    {
        var serializedSubtitles = JsonSerializer.Serialize(
            subtitlesDtos,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(
                    UnicodeRanges.BasicLatin,
                    UnicodeRanges.Cyrillic,
                    UnicodeRanges.Arabic
                ),
            }
        );

        var rawPrompt =
            @"Translate the following subtitles, following the instructions above:
{0}";
        return string.Format(rawPrompt, serializedSubtitles);
    }

    private static ListResult<SubtitleDto> DeserializeSubtitles(
        TranslationRequestDto requestDto,
        string llmResponse
    )
    {
        var llmSubtitles = JsonSerializer.Deserialize<List<SubtitleDto>>(
            llmResponse,
            new JsonSerializerOptions { WriteIndented = true }
        );
        if (llmSubtitles?.Count != requestDto.SourceSubtitles.Count)
        {
            var error = new Error(
                ErrorCode.BadGateway,
                $"Sizes of source and translated subtitles lists do not match. Expected: {requestDto.SourceSubtitles.Count}, but was: {llmSubtitles?.Count}"
            );
            return ListResult<SubtitleDto>.Failure(error);
        }

        if (!ValidateTranslatedSubtitles(requestDto, llmSubtitles))
        {
            var error = new Error(
                ErrorCode.BadGateway,
                $"Translated and original subtitles do not match."
            );
            return ListResult<SubtitleDto>.Failure(error);
        }
        return ListResult<SubtitleDto>.Success(llmSubtitles);
    }

    private static Result<SubtitleDto> DeserializeSubtitle(
        string llmResponse,
        JsonSerializerOptions serializerOptions
    )
    {
        try
        {
            var subtitleDto = JsonSerializer.Deserialize<SubtitleDto>(
                llmResponse,
                serializerOptions
            );

            if (subtitleDto == null)
            {
                var error = new Error(ErrorCode.BadGateway, "Deserialized subtitlte is empty");
                return Result<SubtitleDto>.Failure(error);
            }

            return Result<SubtitleDto>.Success(subtitleDto);
        }
        catch (Exception e)
        {
            var error = new Error(ErrorCode.BadGateway, e.Message);
            return Result<SubtitleDto>.Failure(error);
        }
    }

    private static bool ValidateTranslatedSubtitles(
        TranslationRequestDto requestDto,
        List<SubtitleDto> translatedSubtitleDtos
    )
    {
        for (int i = 0; i < requestDto.SourceSubtitles.Count; i++)
        {
            var sourceSubtitle = requestDto.SourceSubtitles[i];
            var llmSubtitle = translatedSubtitleDtos[i];

            if (
                !ValidateTranslatedSubtitle(
                    requestDto.TargetLanguageCode,
                    sourceSubtitle,
                    llmSubtitle
                )
            )
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateTranslatedSubtitle(
        string targetLanguageCode,
        SubtitleDto subtitleDto,
        SubtitleDto translatedSubtitleDto
    )
    {
        return subtitleDto.StartTime == translatedSubtitleDto.StartTime
            && subtitleDto.EndTime == translatedSubtitleDto.EndTime
            && targetLanguageCode == translatedSubtitleDto.LanguageCode;
    }
}
