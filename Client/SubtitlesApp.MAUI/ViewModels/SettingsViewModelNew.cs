using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.ClientModels.SettingsItems;
using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels;

public partial class SettingsViewModelNew : ObservableObject
{
    private readonly IBuiltInDialogService _dialogService;
    private readonly ITranscriptionClientSettings _transcriptionSettings;
    private readonly IGeminiClientSettings _geminiClientSettings;
    private readonly IOpenAiClientSettings _openAiClientSettings;
    private readonly ILlmSettings _llmClientSettings;

    private readonly ObservableCollection<SettingsItem> _openAiSettings = [];
    private readonly ObservableCollection<SettingsItem> _geminiSettings = [];

    public ObservableCollection<SettingsItemsGroup> SettingsItems { get; } = [];

    public SettingsViewModelNew(
        IBuiltInDialogService dialogService,
        ITranscriptionClientSettings transcriptionSettings,
        IGeminiClientSettings geminiClientSettings,
        IOpenAiClientSettings openAiClientSettings,
        ILlmSettings llmClientSettings
    )
    {
        _dialogService = dialogService;
        _transcriptionSettings = transcriptionSettings;
        _geminiClientSettings = geminiClientSettings;
        _openAiClientSettings = openAiClientSettings;
        _llmClientSettings = llmClientSettings;

        AddTranscriptionSettings();
        AddOpenAiSettings();
        AddGeminiSettings();

        var llmProviderSettings = new PickerSettingsItem(
            dialogService,
            () => llmClientSettings.LlmProvider,
            UpdateLlmProvider
        )
        {
            Title = "LLM Provider",
            AllValues = [LlmProviderConstants.OpenAi, LlmProviderConstants.Gemini],
            SecondaryTextMode = SecondaryTextMode.Value,
        };

        SettingsItems.Add(new SettingsItemsGroup("LLM Translation", [llmProviderSettings]));

        if (llmClientSettings.LlmProvider == LlmProviderConstants.OpenAi)
        {
            SettingsItems.Add(new SettingsItemsGroup("LLM Translation Client", _openAiSettings));
        }
        else if (llmClientSettings.LlmProvider == LlmProviderConstants.Gemini)
        {
            SettingsItems.Add(new SettingsItemsGroup("LLM Translation Client", _geminiSettings));
        }
    }

    private void UpdateLlmProvider(string? newValue)
    {
        var llmClientSettings = SettingsItems.Single(x => x.Name == "LLM Translation Client");

        if (newValue == LlmProviderConstants.OpenAi)
        {
            llmClientSettings.Clear();

            foreach (var settignsItem in _openAiSettings)
            {
                llmClientSettings.Add(settignsItem);
            }
        }
        else if (newValue == LlmProviderConstants.Gemini)
        {
            llmClientSettings.Clear();

            foreach (var settignsItem in _geminiSettings)
            {
                llmClientSettings.Add(settignsItem);
            }
        }
        else
        {
            throw new ArgumentException("Invalid LLM provider", newValue);
        }

        _llmClientSettings.LlmProvider = newValue;
    }

    private void AddTranscriptionSettings()
    {
        var modelSettings = new EntrySettingsItem(
            _dialogService,
            () => _transcriptionSettings.Model,
            (value) => _transcriptionSettings.Model = value
        )
        {
            Title = "Model",
            SecondaryTextMode = SecondaryTextMode.Value,
        };

        var apiKeySettings = new AsyncEntrySettingsItem(
            _dialogService,
            _transcriptionSettings.GetSecret,
            _transcriptionSettings.SetSecret
        )
        {
            Title = "Api Key",
            SecondaryTextMode = SecondaryTextMode.ValueMasked,
        };

        var endpointSettings = new EntrySettingsItem(
            _dialogService,
            () => _transcriptionSettings.Endpoint ?? string.Empty,
            (value) => _transcriptionSettings.Endpoint = value
        )
        {
            Title = "Endpoint",
            Description = "Edit this field only when using self hosted whisper models",
            SecondaryTextMode = SecondaryTextMode.Description,
        };

        SettingsItems.Add(new SettingsItemsGroup("Transcription", [modelSettings, apiKeySettings, endpointSettings]));
    }

    private void AddOpenAiSettings()
    {
        var modelSettings = new EntrySettingsItem(
            _dialogService,
            () => _openAiClientSettings.Model,
            (value) => _openAiClientSettings.Model = value
        )
        {
            Title = "Model",
            SecondaryTextMode = SecondaryTextMode.Value,
        };

        var apiKeySettings = new AsyncEntrySettingsItem(
            _dialogService,
            _openAiClientSettings.GetSecret,
            _openAiClientSettings.SetSecret
        )
        {
            Title = "Api Key",
            SecondaryTextMode = SecondaryTextMode.ValueMasked,
        };

        var endpointSettings = new EntrySettingsItem(
            _dialogService,
            () => _openAiClientSettings.Endpoint ?? string.Empty,
            (value) => _openAiClientSettings.Endpoint = value
        )
        {
            Title = "Endpoint",
            Description = "Edit this field only when using self hosted OpeAi-compatible APIs",
            SecondaryTextMode = SecondaryTextMode.Description,
        };

        _openAiSettings.Add(modelSettings);
        _openAiSettings.Add(apiKeySettings);
        _openAiSettings.Add(endpointSettings);
    }

    private void AddGeminiSettings()
    {
        var modelSettings = new EntrySettingsItem(
            _dialogService,
            () => _geminiClientSettings.Model,
            (value) => _geminiClientSettings.Model = value
        )
        {
            Title = "Model",
            SecondaryTextMode = SecondaryTextMode.Value,
        };

        var apiKeySettings = new AsyncEntrySettingsItem(
            _dialogService,
            _geminiClientSettings.GetSecret,
            _geminiClientSettings.SetSecret
        )
        {
            Title = "Api Key",
            SecondaryTextMode = SecondaryTextMode.ValueMasked,
        };

        _geminiSettings.Add(modelSettings);
        _geminiSettings.Add(apiKeySettings);
    }
}
