namespace SubtitlesApp.ViewModels.SettingsItems;

public abstract class AsyncVirtualSettingsItem<T> : SettingsItem
{
    private readonly Func<Task<T>>? _asyncGetter;
    private readonly Func<T, Task>? _asyncSetter;

    public AsyncVirtualSettingsItem(Func<Task<T>>? asyncGetter = null, Func<T, Task>? asyncSetter = null)
    {
        _asyncGetter = asyncGetter;
        _asyncSetter = asyncSetter;
    }

    protected async Task<T?> GetValueAsync()
    {
        if (_asyncGetter is null)
        {
            return default;
        }

        return _asyncGetter is null ? default : await _asyncGetter.Invoke();
    }

    protected async Task SetValueAsync(T value)
    {
        if (_asyncSetter is null || value is not null && value.Equals(await GetValueAsync()))
        {
            return;
        }

        await _asyncSetter.Invoke(value);
    }
}
