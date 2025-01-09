using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface IHttpRequestService
{
    Task<Result<TResult>> SendAsync<TResult>(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default
    )
        where TResult : class, new();

    Task<AsyncEnumerableResult<TResultItem>> StreamAsync<TResultItem>(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default
    );
}
