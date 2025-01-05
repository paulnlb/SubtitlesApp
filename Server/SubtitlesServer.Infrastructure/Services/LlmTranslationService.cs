using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubtitlesApp.Core.DTOs;
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
        ILlmService lmService,
        ILogger<LlmTranslationService> logger
    )
    {
        _config = ollamaOptions.Value;
        _languageService = languageService;
        _llmService = lmService;
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

        // Insert current language name into system prompt
        var systemPrompt = string.Format(_config.DefaultSystemPrompt, targetLanguage.EnglishName);
        var chatHistory = new List<LlmMessageDto>() { new("system", systemPrompt) };
        var userPrompt = SerializeSubtitlesToPrompt(requestDto.SourceSubtitles);

        _logger.LogInformation("Starting translation with llm...");
        var llmResult = await _llmService.SendAsync(chatHistory, userPrompt);
        _logger.LogInformation("Got response from llm.");
        if (llmResult.IsFailure)
        {
            return ListResult<SubtitleDto>.Failure(llmResult.Error);
        }

        return DeserializeLlmResponseToSubtitles(requestDto, llmResult.Value);
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

    private static ListResult<SubtitleDto> DeserializeLlmResponseToSubtitles(
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

        if (!ValidateLlmSubtitles(requestDto, llmSubtitles))
        {
            var error = new Error(
                ErrorCode.BadGateway,
                $"Translated and original subtitles do not match."
            );
            return ListResult<SubtitleDto>.Failure(error);
        }
        return ListResult<SubtitleDto>.Success(llmSubtitles);
    }

    private static bool ValidateLlmSubtitles(
        TranslationRequestDto requestDto,
        List<SubtitleDto> llmSubtitleDtos
    )
    {
        for (int i = 0; i < requestDto.SourceSubtitles.Count; i++)
        {
            var sourceSubtitle = requestDto.SourceSubtitles[i];
            var llmSubtitle = llmSubtitleDtos[i];

            if (!ValidateLlmSubtitle(requestDto.TargetLanguageCode, sourceSubtitle, llmSubtitle))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateLlmSubtitle(
        string targetLanguageCode,
        SubtitleDto subtitleDto,
        SubtitleDto llmSubtitleDto
    )
    {
        return subtitleDto.StartTime == llmSubtitleDto.StartTime
            && subtitleDto.EndTime == llmSubtitleDto.EndTime
            && targetLanguageCode == llmSubtitleDto.LanguageCode;
    }
}
