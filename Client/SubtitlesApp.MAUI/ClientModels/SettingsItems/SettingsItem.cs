using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.ClientModels.SettingsItems;

public partial class SettingsItem : ObservableObject
{
    private const string ValueMask = "*******";
    private string? _value;
    private string? _description;

    [ObservableProperty]
    public string _title;

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

    public string? Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged(nameof(Value));

                if (SecondaryTextMode == SecondaryTextMode.Value)
                {
                    OnPropertyChanged(nameof(SecondaryText));
                }

                ValueChanged?.Invoke(this, System.EventArgs.Empty);
            }
        }
    }

    public SecondaryTextMode SecondaryTextMode { get; set; }

    public string? SecondaryText
    {
        get
        {
            return this.SecondaryTextMode switch
            {
                SecondaryTextMode.Value => Value,
                SecondaryTextMode.Description => Description,
                SecondaryTextMode.ValueMasked => ValueMask,
                SecondaryTextMode.None => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(this.SecondaryTextMode), "Invalid enum value."),
            };
        }
    }

    public bool IsValueSetAsync { get; set; }

    public event EventHandler? ValueChanged;

    [RelayCommand]
    public virtual Task EditValueAsync()
    {
        return Task.CompletedTask;
    }
}
