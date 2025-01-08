using System.Net.Http.Json;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public class TranslationService : ITranslationService
{
    private readonly IHttpRequestService _httpRequestService;
    private readonly ISettingsService _settingsService;

    public TranslationService(
        ISettingsService settingsService,
        IHttpRequestService httpRequestService
    )
    {
        _httpRequestService = httpRequestService;
        _settingsService = settingsService;
    }

    public async Task<ListResult<SubtitleDto>> TranslateAsync(
        List<SubtitleDto> subtitlesToTranslate,
        string targetLanguageCode,
        CancellationToken cancellationToken = default
    )
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _settingsService.TranslationPath);
        var requestBody = new TranslationRequestDto
        {
            TargetLanguageCode = targetLanguageCode,
            SourceSubtitles = subtitlesToTranslate,
        };
        request.Content = JsonContent.Create(requestBody);

        var result = await _httpRequestService.SendAsync<List<SubtitleDto>>(
            request,
            cancellationToken
        );

        return ListResult<SubtitleDto>.FromGeneric(result);
    }

    public Task<AsyncEnumerableResult<SubtitleDto>> TranslateAndStreamAsync(
        List<SubtitleDto> subtitlesToTranslate,
        string targetLanguageCode,
        CancellationToken cancellationToken = default
    )
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            _settingsService.TranslationStreamPath
        );
        var requestBody = new TranslationRequestDto
        {
            TargetLanguageCode = targetLanguageCode,
            SourceSubtitles = subtitlesToTranslate,
        };
        request.Content = JsonContent.Create(requestBody);

        return _httpRequestService.StreamAsync<SubtitleDto>(request, cancellationToken);
    }
}
