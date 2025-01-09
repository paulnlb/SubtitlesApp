using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public class HttpRequestService : IHttpRequestService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public HttpRequestService(
        IAuthService authService,
        HttpClient client,
        ISettingsService settingsService
    )
    {
        client.BaseAddress = new Uri(settingsService.BackendBaseUrl);
        _httpClient = client;
        _authService = authService;
    }

    public async Task<Result<T>> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default
    )
        where T : class, new()
    {
        try
        {
            var token = await _authService.GetAccessTokenAsync();

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var resultContent =
                    await response.Content.ReadFromJsonAsync<T>(cancellationToken) ?? new();

                return Result<T>.Success(resultContent);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var error =
                    await response.Content.ReadFromJsonAsync<Error>(cancellationToken)
                    ?? new Error(
                        ErrorCode.BadRequest,
                        "Something is wrong with your request to the server"
                    );

                return Result<T>.Failure(error);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var error = new Error(
                    ErrorCode.Unauthorized,
                    "You are not authorized to access this resource"
                );
                return Result<T>.Failure(error);
            }
            else
            {
                var error = new Error(
                    ErrorCode.InternalServerError,
                    "Something has broken on the server side. Please try again later"
                );

                return Result<T>.Failure(error);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return Result<T>.Failure(
                new Error(
                    ErrorCode.ConnectionError,
                    "Error while connecting to the server. Check your connection and try again"
                )
            );
        }
        catch (WebException ex)
        {
            return Result<T>.Failure(
                new Error(
                    ErrorCode.ConnectionError,
                    $"Error while connecting to the server: {ex.Message}"
                )
            );
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(
                new Error(
                    ErrorCode.InternalClientError,
                    $"An unknown error has occurred. {ex.Message}"
                )
            );
        }
    }

    public async Task<AsyncEnumerableResult<T>> StreamAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default
    )
    {
        var token = await _authService.GetAccessTokenAsync();

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var deserializeOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var results = JsonSerializer.DeserializeAsyncEnumerable<T>(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                deserializeOptions,
                cancellationToken: cancellationToken
            );

            return AsyncEnumerableResult<T>.Success(RemoveNulls(results));
        }
        else if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error =
                await response.Content.ReadFromJsonAsync<Error>(cancellationToken)
                ?? new Error(
                    ErrorCode.BadRequest,
                    "Something is wrong with your request to the server"
                );

            return AsyncEnumerableResult<T>.Failure(error);
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var error = new Error(
                ErrorCode.Unauthorized,
                "You are not authorized to access this resource"
            );
            return AsyncEnumerableResult<T>.Failure(error);
        }
        else
        {
            var error = new Error(
                ErrorCode.InternalServerError,
                "Something has broken on the server side. Please try again later"
            );

            return AsyncEnumerableResult<T>.Failure(error);
        }

        static async IAsyncEnumerable<T> RemoveNulls(IAsyncEnumerable<T?> source)
        {
            await foreach (var item in source)
            {
                if (item == null)
                {
                    continue;
                }

                yield return item;
            }
        }
    }
}
