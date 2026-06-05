using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.ClientModels.SettingsItems;

public abstract partial class SettingsItem : ObservableObject
{
    private const string ValueMask = "*******";
    private string? _description;
    private string _value = string.Empty;

    private readonly Func<string> _getter;
    private readonly Action<string> _setter;
    private readonly Func<Task<string>> _asyncGetter;
    private readonly Func<string, Task> _asyncSetter;

    [ObservableProperty]
    public string _title = string.Empty;

    public SecondaryTextMode SecondaryTextMode { get; set; }

    public string? SecondaryText
    {
        get
        {
            return this.SecondaryTextMode switch
            {
                SecondaryTextMode.Value => _getter?.Invoke(),
                SecondaryTextMode.Description => Description,
                SecondaryTextMode.ValueMasked => ValueMask,
                SecondaryTextMode.None => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(this.SecondaryTextMode), "Invalid enum value."),
            };
        }
    }

    public string? Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged(nameof(Description));

                if (SecondaryTextMode == SecondaryTextMode.Description)
                {
                    OnPropertyChanged(nameof(SecondaryText));
                }
            }
        }
    }

    public SettingsItem(
        Func<string>? getter = null,
        Action<string>? setter = null,
        Func<Task<string>>? asyncGetter = null,
        Func<string, Task>? asyncSetter = null
    )
    {
        _getter = getter is null ? () => _value : getter;
        _setter = setter is null ? (newVal) => _value = newVal : setter;
        _asyncGetter = asyncGetter is null ? () => Task.FromResult(_value) : asyncGetter;
        _asyncSetter = asyncSetter is null ? async (newVal) => _value = newVal : asyncSetter;

        OnPropertyChanged(nameof(SecondaryText));
    }

    protected string? GetValue() => _getter?.Invoke();

    protected void SetValue(string value)
    {
        if (value == GetValue())
        {
            return;
        }

        _setter?.Invoke(value);
        ValueChanged?.Invoke(this, new CustomEventArgs.ValueChangedEventArgs(value));

        if (SecondaryTextMode == SecondaryTextMode.Value)
        {
            OnPropertyChanged(nameof(SecondaryText));
        }
    }

    protected async Task<string?> GetValueAsync()
    {
        if (_asyncGetter is null)
        {
            return null;
        }

        return await _asyncGetter.Invoke();
    }

    protected async Task SetValueAsync(string value)
    {
        if (value == await GetValueAsync())
        {
            return;
        }

        if (_asyncSetter is null)
        {
            return;
        }

        await _asyncSetter.Invoke(value);
        ValueChanged?.Invoke(this, new CustomEventArgs.ValueChangedEventArgs(value));

        if (SecondaryTextMode == SecondaryTextMode.Value)
        {
            OnPropertyChanged(nameof(SecondaryText));
        }
    }

    public event EventHandler<CustomEventArgs.ValueChangedEventArgs>? ValueChanged;

    [RelayCommand]
    public abstract Task EditValueAsync();
}
