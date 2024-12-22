using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;
using System.Net.Http.Json;

namespace SubtitlesApp.Services;

public class TranslationService : ITranslationService
{
    private readonly IHttpRequestService<List<SubtitleDTO>> _httpRequestService;
    private readonly ISettingsService _settingsService;

    public TranslationService(
        ISettingsService settingsService,
        IHttpRequestService<List<SubtitleDTO>> httpRequestService)
    {
        _httpRequestService = httpRequestService;
        _settingsService = settingsService;
    }

    public Task<Result<List<SubtitleDTO>>> TranslateAsync(TranslationRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _settingsService.TranslationPath);
        request.Content = JsonContent.Create(requestDto);

        return _httpRequestService.SendAsync(request, cancellationToken);
    }
}
