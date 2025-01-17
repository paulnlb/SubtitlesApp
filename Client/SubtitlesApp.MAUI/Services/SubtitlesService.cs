﻿using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public class SubtitlesService : ISubtitlesService
{
    private readonly IHttpRequestService _httpRequestService;
    private readonly ISettingsService _settingsService;

    public SubtitlesService(ISettingsService settingsService, IHttpRequestService httpRequestService)
    {
        _httpRequestService = httpRequestService;
        _settingsService = settingsService;
    }

    public async Task<ListResult<SubtitleDto>> GetSubsAsync(
        byte[] audioBytes,
        string languageCode,
        TimeSpan? timeOffset = null,
        CancellationToken cancellationToken = default
    )
    {
        var multipartContent = new MultipartFormDataContent
        {
            { new ByteArrayContent(audioBytes), "audioFile", "audio.wav" },
            { new StringContent(languageCode), "languageCode" },
            { new StringContent("true"), "oneSentencePerSubtitle" },
        };

        var request = new HttpRequestMessage(HttpMethod.Post, _settingsService.TranscriptionPath)
        {
            Content = multipartContent,
        };
        request.Headers.ExpectContinue = true;

        var result = await _httpRequestService.SendAsync<List<SubtitleDto>>(request, cancellationToken);

        if (result.IsSuccess && timeOffset.HasValue && timeOffset.Value != TimeSpan.Zero)
        {
            AlignSubsByTime(result.Value, timeOffset.Value);
        }

        return ListResult<SubtitleDto>.FromGeneric(result);
    }

    private static void AlignSubsByTime(List<SubtitleDto> subsToAlign, TimeSpan timeOffset)
    {
        foreach (var subtitleDto in subsToAlign)
        {
            subtitleDto.StartTime += timeOffset;
            subtitleDto.EndTime += timeOffset;
        }
    }
}
