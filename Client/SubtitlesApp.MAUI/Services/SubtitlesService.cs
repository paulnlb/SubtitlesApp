using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Maui.Interfaces;
using System.Net;
using System.Net.Http.Json;

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

    public async Task<Result<List<SubtitleDTO>>> GetSubsAsync(byte[] audioBytes, CancellationToken cancellationToken = default)
    {
        try
        {
            var multipartContent = new MultipartFormDataContent
            {
                { new ByteArrayContent(audioBytes), "audioFile", "audio.wav" }
            };

            var response = await _httpClient.PostAsync(_settingsService.WhisperAddress, multipartContent, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var subtitles = await response.Content.ReadFromJsonAsync<List<SubtitleDTO>>(cancellationToken) ?? [];

                return Result<List<SubtitleDTO>>.Success(subtitles);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadFromJsonAsync<Error>(cancellationToken) 
                    ?? new Error(ErrorCode.BadRequest, "Something is wrong with your request to the server");

                return Result<List<SubtitleDTO>>.Failure(error);
            }
            else
            {
                var error = new Error(
                    ErrorCode.InternalServerError, 
                    "Something has broken on the server side. Please try again later");

                return Result<List<SubtitleDTO>>.Failure(error);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return Result<List<SubtitleDTO>>.Failure(new Error(ErrorCode.ConnectionError, "Error while connecting to the server. Check your connection and try again"));
        }
        catch (WebException ex)
        {
            return Result<List<SubtitleDTO>>.Failure(new Error(ErrorCode.ConnectionError, $"Error while connecting to the server: {ex.Message}"));
        }
        catch (Exception)
        {
            return Result<List<SubtitleDTO>>.Failure(new Error(ErrorCode.InternalClientError, "An unknown error has occurred. Please try again later"));
        }
    }
}
