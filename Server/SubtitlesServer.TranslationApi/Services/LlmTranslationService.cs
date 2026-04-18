using System.IO.Pipelines;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using AutoMapper;
using Microsoft.Extensions.Options;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Core.Services;
using SubtitlesServer.TranslationApi.Configs;
using SubtitlesServer.TranslationApi.Helpers;
using SubtitlesServer.TranslationApi.Interfaces;
using SubtitlesServer.TranslationApi.Models;

namespace SubtitlesServer.TranslationApi.Services;

public class LlmTranslationService : ITranslationService
{
    private readonly LlmTranslationConfig _config;
    private readonly LanguageService _languageService;
    private readonly ILlmService _llmService;
    private readonly ILogger<LlmTranslationService> _logger;
    private readonly IMapper _mapper;

    public LlmTranslationService(
        IOptions<LlmTranslationConfig> llmTranslationOptions,
        LanguageService languageService,
        ILlmService llmService,
        ILogger<LlmTranslationService> logger,
        IMapper mapper
    )
    {
        _config = llmTranslationOptions.Value;
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

        var chatHistory = CreateChatHistory(targetLanguage.Name);
        var userPrompt = MakeUserPrompt(requestDto.SourceSubtitles);
        var responseFormat = GetResponseFormat();

        var llmResult = await _llmService.SendChatAsync(chatHistory, userPrompt, responseFormat);

        if (llmResult.IsFailure)
        {
            return ListResult<SubtitleDto>.Failure(llmResult.Error);
        }

        var translatedSubs = DeserializeSubtitles(llmResult.Value);

        return ListResult<SubtitleDto>.Success(translatedSubs);
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

        var chatHistory = CreateChatHistory(targetLanguage.Name);
        var userPrompt = MakeUserPrompt(requestDto.SourceSubtitles);
        var responseFormat = GetResponseFormat();

        var pipe = new Pipe();
        var llmResult = _llmService.StreamChatAsync(chatHistory, userPrompt, responseFormat);

        if (llmResult.IsFailure)
        {
            return AsyncEnumerableResult<SubtitleDto>.Failure(llmResult.Error);
        }

        _ = WriteLlmPortionsToPipe(llmResult.Value, pipe.Writer);
        var subtitlesEnumerable = DeserializeSubtitlesFromStreamAsync(requestDto, pipe.Reader);

        return AsyncEnumerableResult<SubtitleDto>.Success(subtitlesEnumerable);
    }

    #region Private Methods
    private static JsonNode GetResponseFormat()
    {
        var json = """
            {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "Text": { "type": "string" },
                  "StartTime": { "type": "string" },
                  "EndTime": { "type": "string" },
                  "LanguageCode": { "type": "string" }
                },
                "required": ["Text", "StartTime", "EndTime", "LanguageCode"],
                "additionalProperties": false
              }
            }
            """;

        return JsonNode.Parse(json)!;
    }

    private List<LlmMessageDto> CreateChatHistory(string languageName)
    {
        var systemPrompt = string.Format(_config.DefaultSystemPrompt, languageName);

        return [new("system", systemPrompt)];
    }

    private static List<SubtitleDto> DeserializeSubtitles(string llmResponse)
    {
        return JsonSerializer.Deserialize<List<SubtitleDto>>(
                llmResponse,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(
                        UnicodeRanges.BasicLatin,
                        UnicodeRanges.Cyrillic,
                        UnicodeRanges.Arabic
                    ),
                }
            ) ?? [];
    }

    private async IAsyncEnumerable<SubtitleDto> DeserializeSubtitlesFromStreamAsync(
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

    private static string MakeUserPrompt(List<SubtitleDto> subtitlesDtos)
    {
        var serializedSubtitles = JsonSerializer.Serialize(
            subtitlesDtos,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic, UnicodeRanges.Arabic),
            }
        );

        var rawPrompt = @"Translate the following subtitles:{0}";
        return string.Format(rawPrompt, serializedSubtitles);
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
            stringBuffer.Clear();
        }
    }

    #endregion
}
