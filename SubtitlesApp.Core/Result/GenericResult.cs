namespace SubtitlesApp.Core.Result;

public class Result<T> : Result
{
    private Result(bool isSuccess, Error error, T value)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T Value { get; }

    public static Result<T> Success(T value) => new(true, Error.None, value);

    public new static Result<T> Failure(Error error) => new(false, error, default);
}