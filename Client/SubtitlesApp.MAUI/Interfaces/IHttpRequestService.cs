using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface IHttpRequestService<T> where T : class, new()
{
    Task<Result<T>> SendAsync(
        HttpRequestMessage request,
        HttpClient httpClient,
        CancellationToken cancellationToken = default);
}
