using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
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

    public Task<Result<List<SubtitleDTO>>> GetSubsAsync(
        byte[] audioBytes,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var multipartContent = new MultipartFormDataContent
        {
            { new ByteArrayContent(audioBytes), "audioFile", "audio.wav" }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, _settingsService.TranscriptionPath);

        request.Content = multipartContent;
        request.Headers.Add(RequestConstants.SubtitlesLanguageHeader, languageCode);

        return _httpRequestService.SendAsync(request, cancellationToken);
    }
}
