using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.ViewModels;

public partial class SettingsViewModel(IOpenAiSettings openAiSettings, ITranscriptionClientSettings transcriptionSettings)
    : ObservableObject
{
    [ObservableProperty]
    private string _trancriptionApiKey = transcriptionSettings.ApiKey;

    [ObservableProperty]
    private string _trancriptionEndpoint = transcriptionSettings.Endpoint ?? string.Empty;

    [ObservableProperty]
    private string _transcriptionModel = transcriptionSettings.Model;

    [ObservableProperty]
    private string _openAiApiKey = openAiSettings.ApiKey;

    [ObservableProperty]
    private string _openAiEndpoint = openAiSettings.Endpoint ?? string.Empty;

    [ObservableProperty]
    private string _openAiModel = openAiSettings.Model;

    [RelayCommand]
    public void Save()
    {
        openAiSettings.ApiKey = OpenAiApiKey;
        openAiSettings.Endpoint = OpenAiEndpoint;
        openAiSettings.Model = OpenAiModel;
        transcriptionSettings.ApiKey = TrancriptionApiKey;
        transcriptionSettings.Endpoint = TrancriptionEndpoint;
        transcriptionSettings.Model = TranscriptionModel;
    }
}
