using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Maui.Interfaces;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SubtitlesApp.Services;

public class SubtitlesService : ISubtitlesService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;

    public SubtitlesService(HttpClient httpClient, ISettingsService settingsService)
    {
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri(settingsService.BackendBaseUrl);

        _settingsService = settingsService;
    }

    public async Task<List<SubtitleDTO>> GetSubsAsync(TrimmedAudioDto audioMetadata, CancellationToken cancellationToken = default)
    {
        var httpContent = new StringContent(JsonSerializer.Serialize(audioMetadata), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_settingsService.WhisperAddress, httpContent, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var subtitles = await response.Content.ReadFromJsonAsync<List<SubtitleDTO>>(cancellationToken) ?? [];

            return subtitles;
        }
        else
        {
            return [];
        }
    }
}
