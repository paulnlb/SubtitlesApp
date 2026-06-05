using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.ClientModels.SettingsItems;

public abstract partial class SettingsItem : ObservableObject
{
    protected const string ValueMask = "*******";
    private string? _secondaryText;
    protected SecondaryTextMode SecondaryTextMode;

    [ObservableProperty]
    public string _title = string.Empty;

    [ObservableProperty]
    private string? _description;

    public string? SecondaryText
    {
        get => _secondaryText;
        protected set
        {
            if (_secondaryText != value)
            {
                _secondaryText = value;
                OnPropertyChanged(nameof(SecondaryText));
            }
        }
    }

    public SettingsItem(SecondaryTextMode secondaryTextMode)
    {
        SecondaryTextMode = secondaryTextMode;
        SecondaryText = secondaryTextMode switch
        {
            SecondaryTextMode.Description => Description,
            SecondaryTextMode.ValueMasked => ValueMask,
            SecondaryTextMode.None => null,
            SecondaryTextMode.Value => null,
            _ => "Error: unknown secondary text mode",
        };
    }

    [RelayCommand]
    public abstract Task EditValueAsync();

    partial void OnDescriptionChanged(string? value)
    {
        if (SecondaryTextMode == SecondaryTextMode.Description)
        {
            SecondaryText = value;
        }
    }
}
