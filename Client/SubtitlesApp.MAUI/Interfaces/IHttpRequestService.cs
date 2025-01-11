using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface IHttpRequestService
{
    Task<Result<TResultItem>> SendAsync<TResultItem>(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default
    )
        where TResultItem : class, new();

    Task<AsyncEnumerableResult<TResultItem>> StreamAsync<TResultItem>(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default
    );
}
