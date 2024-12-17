using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public class SubtitlesService : ISubtitlesService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpRequestService<List<SubtitleDTO>> _httpRequestService;
    private readonly ISettingsService _settingsService;

    public SubtitlesService(
        HttpClient httpClient, 
        ISettingsService settingsService,
        IHttpRequestService<List<SubtitleDTO>> httpRequestService)
    {
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri(settingsService.BackendBaseUrl);

        _httpRequestService = httpRequestService;
        _settingsService = settingsService;
    }

    public Task<Result<List<SubtitleDTO>>> GetSubsAsync(
        byte[] audioBytes,
        Language language,
        CancellationToken cancellationToken = default)
    {
        var multipartContent = new MultipartFormDataContent
        {
            { new ByteArrayContent(audioBytes), "audioFile", "audio.wav" }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _settingsService.TranscriptionPath);

        request.Content = multipartContent;
        request.Headers.Add(RequestConstants.SubtitlesLanguageHeader, language.Code);

        return _httpRequestService.SendAsync(request, _httpClient, cancellationToken);
    }
}
