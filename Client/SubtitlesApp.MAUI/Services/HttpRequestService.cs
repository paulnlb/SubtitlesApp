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

    public HttpRequestService(IAuthService authService, HttpClient client, ISettingsService settingsService)
    {
        client.BaseAddress = new Uri(settingsService.BackendBaseUrl);
        _httpClient = client;
        _authService = authService;
    }

    public Task<Result> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        return SendAsyncInternal(request, successResponse => Task.FromResult(Result.Success()), cancellationToken);
    }

    public Task<Result<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        return SendAsyncInternal(
            request,
            async successResponse =>
            {
                var responseContent = await successResponse.Content.ReadFromJsonAsync<T>(cancellationToken) ?? new();
                return Result<T>.Success(responseContent);
            },
            cancellationToken
        );
    }

    public Task<AsyncEnumerableResult<T>> StreamAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default
    )
    {
        return SendAsyncInternal(
            request,
            successResponse =>
            {
                var deserializeOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var deserializedObjects = successResponse.Content.ReadFromJsonAsAsyncEnumerable<T>(
                    deserializeOptions,
                    cancellationToken: cancellationToken
                );
                var result = AsyncEnumerableResult<T>.Success(RemoveNullsFrom(deserializedObjects));

                return Task.FromResult(result);
            },
            cancellationToken
        );

        static async IAsyncEnumerable<T> RemoveNullsFrom(IAsyncEnumerable<T?> source)
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

    private async Task<TResult> SendAsyncInternal<TResult>(
        HttpRequestMessage request,
        Func<HttpResponseMessage, Task<TResult>> successResponseHandler,
        CancellationToken cancellationToken = default
    )
        where TResult : Result
    {
        try
        {
            var token = await _authService.GetAccessTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await successResponseHandler(response);
            }

            var error = await ConvertToErrorAsync(response, cancellationToken);

            // If token expired, try to refresh it and then call this method recursively
            if (error.Code == ErrorCode.TokenExpired)
            {
                var refreshResult = await _authService.RefreshAccessTokenAsync();

                if (refreshResult.IsSuccess)
                {
                    return await SendAsyncInternal(await request.CloneAsync(), successResponseHandler, cancellationToken);
                }
                else
                {
                    error = refreshResult.Error;
                }
            }

            return FailedResultFromError<TResult>(error);
        }
        catch (Exception ex)
        {
            var error = ConvertExceptionToError(ex);
            return FailedResultFromError<TResult>(error);
        }
    }

    private static async Task<Error> ConvertToErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<Error>(cancellationToken);
            return error ?? ConvertStatusCodeToError(response.StatusCode);
        }
        catch
        {
            return ConvertStatusCodeToError(response.StatusCode);
        }
    }

    private static Error ConvertStatusCodeToError(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => new(ErrorCode.BadRequest, "Something is wrong with your request to the server"),
            HttpStatusCode.Unauthorized => new(
                ErrorCode.Unauthorized,
                "Authentication error. Check your credentials and try again"
            ),
            HttpStatusCode.Forbidden => new(ErrorCode.Forbidden, "You cannot access this resource"),
            _ => new(ErrorCode.InternalServerError, "Something has broken on the server side. Please try again later"),
        };
    }

    private static Error ConvertExceptionToError(Exception ex)
    {
        return ex switch
        {
            OperationCanceledException => new(ErrorCode.OperationCanceled, "Operation cancelled"),
            HttpRequestException => new(
                ErrorCode.ConnectionError,
                "Error while connecting to the server. Check your connection and try again"
            ),
            WebException => new(ErrorCode.ConnectionError, $"Error while connecting to the server: {ex.Message}"),
            _ => new(ErrorCode.InternalClientError, $"An unknown error has occurred. {ex.Message}"),
        };
    }

    private static TResult FailedResultFromError<TResult>(Error error)
        where TResult : Result
    {
        var resultObject = typeof(TResult).GetMethod(nameof(Result.Failure))?.Invoke(null, [error]);

        return resultObject as TResult ?? throw new InvalidOperationException();
    }
}
