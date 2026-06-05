using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.ClientModels.SettingsItems;

public abstract partial class SettingsItem : ObservableObject
{
    protected const string ValueMask = "*******";
    private string? _secondaryText;

    [ObservableProperty]
    public string _title = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private SecondaryTextMode _secondaryTextMode;

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

    [RelayCommand]
    public abstract Task EditValueAsync();

    protected abstract void SecondaryTextModeChangeHanlder(SecondaryTextMode value);

    partial void OnDescriptionChanged(string? value)
    {
        if (SecondaryTextMode == SecondaryTextMode.Description)
        {
            SecondaryText = value;
        }
    }

    partial void OnSecondaryTextModeChanged(SecondaryTextMode value)
    {
        SecondaryTextModeChangeHanlder(value);
    }
}
