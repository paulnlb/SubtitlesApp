using Pizda = SubtitlesApp.Core.Result.Result<System.Collections.Generic.List<SubtitlesApp.Core.DTOs.SubtitleDTO>>;

namespace SubtitlesApp.Core.Result;

public class Result<T> : Result
{
    protected Result(bool isSuccess, Error error, T value)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T Value { get; }

    public static Result<T> Success(T value) => new(true, Error.None, value);

    public static new Result<T> Failure(Error error) => new(false, error, default);
}
