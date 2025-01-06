using System.Collections.ObjectModel;

namespace SubtitlesApp.Core.Result;

public class ObservableCollectionResult<T> : Result<ObservableCollection<T>>
{
    private ObservableCollectionResult(bool isSuccess, Error error, ObservableCollection<T> value)
        : base(isSuccess, error, value) { }

    public static new ObservableCollectionResult<T> Success(ObservableCollection<T> value) =>
        new(true, Error.None, value);

    public static new ObservableCollectionResult<T> Failure(Error error) =>
        new(false, error, default);

    public static ObservableCollectionResult<T> FromGeneric(
        Result<ObservableCollection<T>> result
    ) => new(result.IsSuccess, result.Error, result.Value);
}
