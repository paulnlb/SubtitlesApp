using System.IO.Pipelines;
using System.Text;
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

        var systemPrompt = string.Format(_config.DefaultSystemPrompt, targetLanguage.Name);
        var chatHistory = new List<LlmMessageDto>() { new("system", systemPrompt) };
        var userPrompt = SerializeSubtitlesToPrompt(requestDto.SourceSubtitles);

        var pipe = new Pipe();
        var llmResponsePortions = _llmService.StreamAsync(chatHistory, userPrompt);
        _ = WriteLlmPortionsToPipe(llmResponsePortions, pipe.Writer);
        var subtitlesEnumerable = DeserializeSubtitlesFromSteamAsync(requestDto, pipe.Reader);

        return AsyncEnumerableResult<SubtitleDto>.Success(subtitlesEnumerable);
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
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic, UnicodeRanges.Arabic),
            }
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
            var error = new Error(ErrorCode.BadGateway, $"Original subtitles and translations do not match.");
            return ListResult<SubtitleDto>.Failure(error);
        }

        var translatedSubtitlesDtos = _mapper.Map<List<SubtitleDto>>(requestDto.SourceSubtitles);
        _mapper.Map(llmTranslations, translatedSubtitlesDtos);

        return ListResult<SubtitleDto>.Success(translatedSubtitlesDtos);
    }

    private async IAsyncEnumerable<SubtitleDto> DeserializeSubtitlesFromSteamAsync(
        TranslationRequestDto requestDto,
        PipeReader reader
    )
    {
        var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var currentSubtitleIndex = 0;

        var translationsEnumerable = JsonSerializer.DeserializeAsyncEnumerable<Translation>(
            reader.AsStream(),
            serializerOptions
        );

        await foreach (var translation in translationsEnumerable)
        {
            var subtitleDto = requestDto.SourceSubtitles[currentSubtitleIndex];

            if (requestDto.TargetLanguageCode != translation?.LanguageCode)
            {
                _logger.LogError(
                    "Translation does not match the source subtitle. Source subtitle: {src}. Translation: {trs}",
                    subtitleDto,
                    translation
                );
                yield break;
            }

            currentSubtitleIndex++;

            var translatedSubtitle = _mapper.Map<SubtitleDto>(subtitleDto);
            _mapper.Map(translation, translatedSubtitle);

            yield return translatedSubtitle;
        }
    }

    private static bool ValidateTranslations(TranslationRequestDto requestDto, List<Translation> translations)
    {
        for (int i = 0; i < translations.Count; i++)
        {
            var translation = translations[i];

            if (requestDto.TargetLanguageCode != translation?.LanguageCode)
            {
                return false;
            }
        }

        return true;
    }

    // Besides of just writing, this method does additional buffering to increase the size of written chunks,
    // because otherwise JsonSerializer.DeserializeAsyncEnumerable method would idle,
    // waiting for the full response instead of deserializing it on the fly.
    // That's because the portions of data streamed by LLMs are too small (typically couple of characters)
    // ref: https://github.com/dotnet/runtime/issues/63864#issuecomment-1016568621
    private async Task WriteLlmPortionsToPipe(IAsyncEnumerable<string> portions, PipeWriter writer)
    {
        var stringBuffer = new StringBuilder();

        try
        {
            await foreach (var portion in portions)
            {
                stringBuffer.Append(portion);

                if (portion.Contains('}'))
                {
                    await WriteBufferToPipe();
                }
            }

            if (stringBuffer.Length > 0)
            {
                await WriteBufferToPipe();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured during streaming LLM response: {err}", ex.Message);
        }
        finally
        {
            await writer.CompleteAsync();
        }

        async Task WriteBufferToPipe()
        {
            var bytes = Encoding.UTF8.GetBytes(stringBuffer.ToString());
            await writer.WriteAsync(bytes);
            Console.Write(stringBuffer.ToString());
            stringBuffer.Clear();
        }
    }
}
