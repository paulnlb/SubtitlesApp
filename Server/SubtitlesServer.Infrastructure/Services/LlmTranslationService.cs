using System.Text.Json;
using AutoMapper;
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
    private readonly ILlmService _lmService;
    private readonly IMapper _mapper;

    public LlmTranslationService(
        IOptions<OllamaConfig> ollamaOptions,
        LanguageService languageService,
        ILlmService lmService,
        IMapper mapper
    )
    {
        _config = ollamaOptions.Value;
        _languageService = languageService;
        _lmService = lmService;
        _mapper = mapper;
    }

    public async Task<Result<List<SubtitleDTO>>> TranslateAsync(TranslationRequestDto requestDto)
    {
        var targetLanguage = _languageService.GetLanguageByCode(requestDto.TargetLanguageCode);

        if (targetLanguage == null)
        {
            var error = new Error(
                ErrorCode.BadRequest,
                $"Target language code {requestDto.TargetLanguageCode} is invalid or not supported"
            );
            return Result<List<SubtitleDTO>>.Failure(error);
        }

        if (requestDto.SourceSubtitles == null || requestDto.SourceSubtitles.Count == 0)
        {
            var error = new Error(
                ErrorCode.BadRequest,
                "Provide at least one subtitle to translate"
            );
            return Result<List<SubtitleDTO>>.Failure(error);
        }

        // Insert current language name into system prompt
        var systemPrompt = string.Format(_config.DefaultSystemPrompt, targetLanguage.EnglishName);

        var chatHistory = new List<LlmMessageDto>() { new("system", systemPrompt) };

        var userPrompt = SerializeSubtitlesToPrompt(requestDto.SourceSubtitles);

        var llmResponse = await _lmService.SendAsync(chatHistory, userPrompt);

        return DeserializeLlmResponseToSubtitles(requestDto, llmResponse);
    }

    private string SerializeSubtitlesToPrompt(List<SubtitleDTO> subtitles)
    {
        var llmSubtitles = _mapper.Map<List<LlmSubtitleDto>>(subtitles);

        var serializerOptions = new JsonSerializerOptions { WriteIndented = true };

        var serializedSubtitles = JsonSerializer.Serialize(llmSubtitles, serializerOptions);

        var rawPrompt =
            @"Translate the following subtitles, following the instructions above:
{0}";
        return string.Format(rawPrompt, serializedSubtitles);
    }

    private Result<List<SubtitleDTO>> DeserializeLlmResponseToSubtitles(
        TranslationRequestDto requestDto,
        string llmResponse
    )
    {
        var serializerOptions = new JsonSerializerOptions { WriteIndented = true };

        var sourceSubtitles = requestDto.SourceSubtitles;
        var llmSubtitles = JsonSerializer.Deserialize<List<LlmSubtitleDto>>(
            llmResponse,
            serializerOptions
        );
        var translatedSubtitles = new List<SubtitleDTO>();

        if (llmSubtitles?.Count != requestDto.SourceSubtitles.Count)
        {
            var error = new Error(
                ErrorCode.DeserializationError,
                $"Sizes of source and translated subtitles lists do not match. Expected: {requestDto.SourceSubtitles.Count}, but was: {llmSubtitles?.Count}"
            );
            return Result<List<SubtitleDTO>>.Failure(error);
        }

        for (int i = 0; i < sourceSubtitles.Count; i++)
        {
            var sourceSubtitle = sourceSubtitles[i];
            var llmSubtitle = llmSubtitles[i];

            if (!ValidateLlmSubtitle(requestDto.TargetLanguageCode, sourceSubtitle, llmSubtitle))
            {
                var error = new Error(
                    ErrorCode.DeserializationError,
                    $"Translated and original subtitles do not match at index {i}"
                );
                return Result<List<SubtitleDTO>>.Failure(error);
            }

            var translatedSubtitle = _mapper.Map<SubtitleDTO>(sourceSubtitle);
            _mapper.Map(llmSubtitle, translatedSubtitle);

            translatedSubtitles.Add(translatedSubtitle);
        }

        return Result<List<SubtitleDTO>>.Success(translatedSubtitles);
    }

    private static bool ValidateLlmSubtitle(
        string targetLanguageCode,
        SubtitleDTO subtitle,
        LlmSubtitleDto llmSubtitle
    )
    {
        return subtitle.TimeInterval.StartTime == llmSubtitle.StartTime
            && subtitle.TimeInterval.EndTime == llmSubtitle.EndTime
            && targetLanguageCode == llmSubtitle.LanguageCode;
    }
}
