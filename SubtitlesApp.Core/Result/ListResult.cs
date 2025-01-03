namespace SubtitlesApp.Core.Result;

public class ListResult<T> : Result<List<T>>
{
    private ListResult(bool isSuccess, Error error, List<T> value)
        : base(isSuccess, error, value) { }

    public static new ListResult<T> Success(List<T> value) => new(true, Error.None, value);

    public static new ListResult<T> Failure(Error error) => new(false, error, default);
}
