namespace SubtitlesApp.Core.Result;

public class AsyncEnumerableResult<T> : Result<IAsyncEnumerable<T>>
{
    private AsyncEnumerableResult(bool isSuccess, Error error, IAsyncEnumerable<T> value)
        : base(isSuccess, error, value) { }

    public static new AsyncEnumerableResult<T> Success(IAsyncEnumerable<T> value) =>
        new(true, Error.None, value);

    public static new AsyncEnumerableResult<T> Failure(Error error) => new(false, error, default);

    public static AsyncEnumerableResult<T> FromGeneric(Result<IAsyncEnumerable<T>> result) =>
        new(result.IsSuccess, result.Error, result.Value);
}
