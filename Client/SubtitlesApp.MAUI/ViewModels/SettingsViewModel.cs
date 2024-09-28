using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.Maui.Interfaces;
using SubtitlesApp.Settings;

namespace SubtitlesApp.ViewModels;

public class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly bool _isDevelopment;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _isDevelopment = settingsService is SettingsServiceDevelopment;
    }

    public string BackendBaseUrl
    {
        get => _settingsService.BackendBaseUrl;
        set
        {
            _settingsService.BackendBaseUrl = value;
            OnPropertyChanged();
        }
    }

    public int TranscribeBufferLength
    {
        get => _settingsService.TranscribeBufferLength;
        set
        {
            _settingsService.TranscribeBufferLength = value;
            OnPropertyChanged();
        }
    }

    public bool IsDevelopment => _isDevelopment;

    public string WhisperAddress
    {
        get => _settingsService.WhisperAddress;
        set
        {
            _settingsService.WhisperAddress = value;
            OnPropertyChanged();
        }
    }
}
