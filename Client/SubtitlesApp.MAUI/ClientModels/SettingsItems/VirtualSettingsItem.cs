namespace SubtitlesApp.ClientModels.SettingsItems;

public abstract partial class VirtualSettingsItem<T> : SettingsItem
{
    private readonly Func<T>? _getter;
    private readonly Action<T>? _setter;

    public VirtualSettingsItem(Func<T>? getter = null, Action<T>? setter = null)
    {
        _getter = getter;
        _setter = setter;
    }

    protected T? GetValue() => _getter is null ? default : _getter.Invoke();

    protected void SetValue(T value)
    {
        if (_setter is null || value is not null && value.Equals(GetValue()))
        {
            return;
        }

        _setter.Invoke(value);
    }
}
