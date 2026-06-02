using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kotlin.Properties;
using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.ViewModels;

public partial class SettingsViewModel(
    ILlmClientSettings llmClientSettings,
    ITranscriptionClientSettings transcriptionSettings
) : ObservableObject
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
    private string _openAiEndpoint = llmClientSettings.OpenAiEndpoint ?? string.Empty;

    [ObservableProperty]
    private string _openAiModel = llmClientSettings.OpenAiModel;

    [ObservableProperty]
    private string _geminiApiKey = NonFetchedKeyPlaceholder;

    [ObservableProperty]
    private string _geminiModel = llmClientSettings.GeminiModel;

    [ObservableProperty]
    private bool _isOpenAiKeyShown;

    [ObservableProperty]
    private bool _isTranscriptionKeyShown;

    [ObservableProperty]
    private bool _isGeminiKeyShown;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _llmProvider = llmClientSettings.LlmProvider;

    [RelayCommand]
    public async Task Save()
    {
        if (OpenAiApiKey != NonFetchedKeyPlaceholder)
        {
            await llmClientSettings.SetOpenAiApiKey(OpenAiApiKey);
        }

        if (GeminiApiKey != NonFetchedKeyPlaceholder)
        {
            await llmClientSettings.SetGeminiApiKey(GeminiApiKey);
        }

        llmClientSettings.OpenAiEndpoint = OpenAiEndpoint;
        llmClientSettings.OpenAiModel = OpenAiModel;

        llmClientSettings.GeminiModel = GeminiModel;

        llmClientSettings.LlmProvider = LlmProvider;

        if (TranscriptionApiKey != NonFetchedKeyPlaceholder)
        {
            await transcriptionSettings.SetApiKey(TranscriptionApiKey);
        }
        transcriptionSettings.Endpoint = TranscriptionEndpoint;
        transcriptionSettings.Model = TranscriptionModel;

        IsDirty = false;
    }

    [RelayCommand]
    public void LlmProviderChanged(string llmProviderName)
    {
        LlmProvider = llmProviderName;
    }

    [RelayCommand]
    public async Task ShowOpenAiLlmKey()
    {
        if (!IsOpenAiKeyShown && OpenAiApiKey == NonFetchedKeyPlaceholder)
        {
            OpenAiApiKey = await llmClientSettings.GetOpenAiApiKey();
        }
        IsOpenAiKeyShown = !IsOpenAiKeyShown;
    }

    [RelayCommand]
    public async Task ShowGeminiLlmKey()
    {
        if (!IsGeminiKeyShown && GeminiApiKey == NonFetchedKeyPlaceholder)
        {
            GeminiApiKey = await llmClientSettings.GetGeminiApiKey();
        }
        IsGeminiKeyShown = !IsGeminiKeyShown;
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

    partial void OnGeminiApiKeyChanged(string? oldValue, string newValue)
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

    partial void OnGeminiModelChanged(string? oldValue, string newValue)
    {
        IsDirty = true;
    }

    partial void OnTranscriptionModelChanged(string? oldValue, string newValue)
    {
        IsDirty = true;
    }

    partial void OnLlmProviderChanged(string? oldValue, string newValue)
    {
        IsDirty = true;
    }
}
