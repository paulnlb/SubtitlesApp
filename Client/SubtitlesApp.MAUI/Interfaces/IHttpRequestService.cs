using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface IHttpRequestService
{
    /// <summary>
    /// Sends an HTTP request and retrieves the result.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Result"/> indicating the outcome of the operation.</returns>
    Task<Result> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP request and retrieves the result, with the response deserialized into a specified type.
    /// </summary>
    /// <typeparam name="TResultItem">The type into which the response will be deserialized. Must be a reference type with a parameterless constructor.</typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Result{TResultItem}"/> containing the deserialized response or an error.</returns>
    Task<Result<TResultItem>> SendAsync<TResultItem>(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default
    )
        where TResultItem : class, new();

    /// <summary>
    /// Sends an HTTP request and retrieves the result as a stream of items of a specified type.
    /// </summary>
    /// <typeparam name="TResultItem">The type of items in the response stream.</typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An <see cref="AsyncEnumerableResult{TResultItem}"/> containing the streamed response items or an error.</returns>
    Task<AsyncEnumerableResult<TResultItem>> StreamAsync<TResultItem>(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default
    );
}
