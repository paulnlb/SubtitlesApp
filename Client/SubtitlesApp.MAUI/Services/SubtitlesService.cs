using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public class SubtitlesService : ISubtitlesService
{
    private readonly IHttpRequestService<List<SubtitleDTO>> _httpRequestService;
    private readonly ISettingsService _settingsService;

    public SubtitlesService(
        ISettingsService settingsService,
        IHttpRequestService<List<SubtitleDTO>> httpRequestService)
    {
        _httpRequestService = httpRequestService;
        _settingsService = settingsService;
    }

    public async Task<Result<List<SubtitleDTO>>> GetSubsAsync(
        byte[] audioBytes,
        string languageCode,
        TimeSpan? timeOffset = null,
        CancellationToken cancellationToken = default)
    {
        var multipartContent = new MultipartFormDataContent
        {
            { new ByteArrayContent(audioBytes), "audioFile", "audio.wav" }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, _settingsService.TranscriptionPath);

        request.Content = multipartContent;
        request.Headers.Add(RequestConstants.SubtitlesLanguageHeader, languageCode);

        var result = await _httpRequestService.SendAsync(request, cancellationToken);

        if (result.IsSuccess && timeOffset.HasValue && timeOffset.Value != TimeSpan.Zero)
        {
            AlignSubsByTime(result.Value, timeOffset.Value);
        }

        return result;
    }

    static void AlignSubsByTime(
        List<SubtitleDTO> subsToAlign,
        TimeSpan timeOffset)
    {
        foreach (var subtitleDto in subsToAlign)
        {
            var timeInterval = new TimeIntervalDTO
            {
                StartTime = subtitleDto.TimeInterval.StartTime + timeOffset,
                EndTime = subtitleDto.TimeInterval.EndTime + timeOffset,
            };

            subtitleDto.TimeInterval = timeInterval;
        }
    }
}
