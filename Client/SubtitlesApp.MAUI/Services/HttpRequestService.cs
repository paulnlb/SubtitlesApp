using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Extensions;
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
                var responseContent =
                    await response.Content.ReadFromJsonAsync<T>(cancellationToken) ?? new();

                return Result<T>.Success(responseContent);
            }

            var error = await ConvertToErrorAsync(response, cancellationToken);

            // If token expired, try to refresh it and then call SendAsync again
            if (error.Code == ErrorCode.TokenExpired)
            {
                var refreshResult = await _authService.RefreshAccessTokenAsync();

                if (refreshResult.IsSuccess)
                {
                    return await SendAsync<T>(await request.CloneAsync(), cancellationToken);
                }
                else
                {
                    error = refreshResult.Error;
                }
            }

            return Result<T>.Failure(error);
        }
        catch (Exception ex)
        {
            var error = ConvertExceptionToError(ex);
            return Result<T>.Failure(error);
        }
    }

    public async Task<AsyncEnumerableResult<T>> StreamAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default
    )
    {
        try
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

            var error = await ConvertToErrorAsync(response, cancellationToken);

            // If token expired, try to refresh it and then call SendAsync again
            if (error.Code == ErrorCode.TokenExpired)
            {
                var refreshResult = await _authService.RefreshAccessTokenAsync();

                if (refreshResult.IsSuccess)
                {
                    return await StreamAsync<T>(await request.CloneAsync(), cancellationToken);
                }
                else
                {
                    error = refreshResult.Error;
                }
            }

            return AsyncEnumerableResult<T>.Failure(error);
        }
        catch (Exception ex)
        {
            var error = ConvertExceptionToError(ex);
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

    private async Task<Error> ConvertToErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default
    )
    {
        Error error;
        if (response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Cannot convert successful response to error");
        }
        else if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            error =
                await response.Content.ReadFromJsonAsync<Error>(cancellationToken)
                ?? new Error(
                    ErrorCode.BadRequest,
                    "Something is wrong with your request to the server"
                );
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            error =
                await response.Content.ReadFromJsonAsync<Error>(cancellationToken)
                ?? new Error(
                    ErrorCode.Unauthorized,
                    "Authentication error. Check your credentials and try again"
                );
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            error =
                await response.Content.ReadFromJsonAsync<Error>(cancellationToken)
                ?? new Error(ErrorCode.Forbidden, "You cannot access this resource");
        }
        else
        {
            error = new Error(
                ErrorCode.InternalServerError,
                "Something has broken on the server side. Please try again later"
            );
        }

        return error;
    }

    private Error ConvertExceptionToError(Exception ex)
    {
        if (ex is OperationCanceledException)
        {
            return new Error(ErrorCode.OperationCanceled, "Operation cancelled");
        }
        else if (ex is HttpRequestException)
        {
            return new Error(
                ErrorCode.ConnectionError,
                "Error while connecting to the server. Check your connection and try again"
            );
        }
        else if (ex is WebException)
        {
            return new Error(
                ErrorCode.ConnectionError,
                $"Error while connecting to the server: {ex.Message}"
            );
        }
        else
        {
            return new Error(
                ErrorCode.InternalClientError,
                $"An unknown error has occurred. {ex.Message}"
            );
        }
    }
}
