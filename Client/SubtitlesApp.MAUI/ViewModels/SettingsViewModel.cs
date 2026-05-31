using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.ViewModels;

public partial class SettingsViewModel(IOpenAiSettings openAiSettings, ITranscriptionClientSettings transcriptionSettings)
    : ObservableObject
{
    private const string NonFetchedKeyPlaceholder = "Forget-Want6-While-Shore-Stage";

    [ObservableProperty]
    private string _transcriptionApiKey = NonFetchedKeyPlaceholder;

    [ObservableProperty]
    private string _transcriptionEndpoint = transcriptionSettings.Endpoint ?? string.Empty;

    [ObservableProperty]
    private string _transcriptionModel = transcriptionSettings.Model;

    [ObservableProperty]
    private string _openAiApiKey = NonFetchedKeyPlaceholder;

    [ObservableProperty]
    private string _openAiEndpoint = openAiSettings.Endpoint ?? string.Empty;

    [ObservableProperty]
    private string _openAiModel = openAiSettings.Model;

    [ObservableProperty]
    private bool _isOpenAiKeyShown;

    [ObservableProperty]
    private bool _isTranscriptionKeyShown;

    [ObservableProperty]
    private bool _isDirty;

    [RelayCommand]
    public async Task Save()
    {
        if (OpenAiApiKey != NonFetchedKeyPlaceholder)
        {
            await openAiSettings.SetApiKey(OpenAiApiKey);
        }

        openAiSettings.Endpoint = OpenAiEndpoint;
        openAiSettings.Model = OpenAiModel;

        if (TranscriptionApiKey != NonFetchedKeyPlaceholder)
        {
            await transcriptionSettings.SetApiKey(TranscriptionApiKey);
        }
        transcriptionSettings.Endpoint = TranscriptionEndpoint;
        transcriptionSettings.Model = TranscriptionModel;
        IsDirty = false;
    }

    [RelayCommand]
    public async Task ShowLlmKey()
    {
        if (!IsOpenAiKeyShown && OpenAiApiKey == NonFetchedKeyPlaceholder)
        {
            OpenAiApiKey = await openAiSettings.GetApiKey();
        }
        IsOpenAiKeyShown = !IsOpenAiKeyShown;
    }

    [RelayCommand]
    public async Task ShowTranscriptionKey()
    {
        if (!IsTranscriptionKeyShown && TranscriptionApiKey == NonFetchedKeyPlaceholder)
        {
            TranscriptionApiKey = await transcriptionSettings.GetApiKey();
        }
        IsTranscriptionKeyShown = !IsTranscriptionKeyShown;
    }

    partial void OnOpenAiApiKeyChanged(string? oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(newValue))
        {
            IsDirty = false;
            return;
        }

        if (oldValue != NonFetchedKeyPlaceholder)
        {
            IsDirty = true;
        }
    }

    partial void OnTranscriptionApiKeyChanged(string? oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(newValue))
        {
            IsDirty = false;
            return;
        }

        if (oldValue != NonFetchedKeyPlaceholder)
        {
            IsDirty = true;
        }
    }

    partial void OnOpenAiEndpointChanged(string? oldValue, string newValue)
    {
        IsDirty = true;
    }

    partial void OnTranscriptionEndpointChanged(string? oldValue, string newValue)
    {
        IsDirty = true;
    }

    partial void OnOpenAiModelChanged(string? oldValue, string newValue)
    {
        IsDirty = true;
    }

    partial void OnTranscriptionModelChanged(string? oldValue, string newValue)
    {
        IsDirty = true;
    }
}
