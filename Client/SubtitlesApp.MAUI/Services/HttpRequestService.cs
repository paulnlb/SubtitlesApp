using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;

namespace SubtitlesApp.Services;

public class HttpRequestService<T> : IHttpRequestService<T> where T : class, new()
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public HttpRequestService(
        IAuthService authService,
        HttpClient client,
        ISettingsService settingsService)
    {
        client.BaseAddress = new Uri(settingsService.BackendBaseUrl);
        _httpClient = client;
        _authService = authService;
    }

    public async Task<Result<T>> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _authService.GetAccessTokenAsync();

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var resultContent = await response.Content.ReadFromJsonAsync<T>(cancellationToken) ?? new();

                return Result<T>.Success(resultContent);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadFromJsonAsync<Error>(cancellationToken)
                    ?? new Error(ErrorCode.BadRequest, "Something is wrong with your request to the server");

                return Result<T>.Failure(error);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var error = new Error(ErrorCode.Unauthorized, "You are not authorized to access this resource");
                return Result<T>.Failure(error);
            }
            else
            {
                var error = new Error(
                    ErrorCode.InternalServerError,
                    "Something has broken on the server side. Please try again later");

                return Result<T>.Failure(error);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return Result<T>.Failure(new Error(ErrorCode.ConnectionError, "Error while connecting to the server. Check your connection and try again"));
        }
        catch (WebException ex)
        {
            return Result<T>.Failure(new Error(ErrorCode.ConnectionError, $"Error while connecting to the server: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(new Error(ErrorCode.InternalClientError, $"An unknown error has occurred. {ex.Message}"));
        }
    }
}
