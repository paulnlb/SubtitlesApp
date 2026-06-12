using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SubtitlesApp.ClientModels.SettingsItems;

public abstract partial class SettingsItem : ObservableObject
{
    [ObservableProperty]
    public string _title = string.Empty;

    [ObservableProperty]
    private string? _subTitle = string.Empty;

    [RelayCommand]
    public abstract Task EditValueAsync();
}
