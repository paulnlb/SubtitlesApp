using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels;

public partial class SettingsViewModel(ISettingsService settingsService) : ObservableObject
{
    public string BackendBaseUrl
    {
        get => settingsService.BackendBaseUrl;
        set
        {
            settingsService.BackendBaseUrl = value;
            OnPropertyChanged();
        }
    }

    public int TranscribeBufferLength
    {
        get => settingsService.TranscribeBufferLength;
        set
        {
            settingsService.TranscribeBufferLength = value;
            OnPropertyChanged();
        }
    }
}
