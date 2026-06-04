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

        var llmProviderSettings = new PickerSettingsItem(dialogService)
        {
            Title = "LLM Provider",
            AllValues = [LlmProviderConstants.OpenAi, LlmProviderConstants.Gemini],
            Value = llmClientSettings.LlmProvider,
        };
        llmProviderSettings.ValueChanged += OnLlmProviderChanged;

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

    private void OnLlmProviderChanged(object? sender, EventArgs e)
    {
        var llmProviderSettings = SettingsItems
            .Single(x => x.Name == "LLM Translation")
            .Single(x => x.Title == "LLM Provider");

        var llmClientSettings = SettingsItems.Single(x => x.Name == "LLM Translation Client");
        llmClientSettings.Clear();

        if (llmProviderSettings.Value == LlmProviderConstants.OpenAi)
        {
            foreach (var settignsItem in _openAiSettings)
            {
                llmClientSettings.Add(settignsItem);
            }
        }
        else if (llmProviderSettings.Value == LlmProviderConstants.Gemini)
        {
            foreach (var settignsItem in _geminiSettings)
            {
                llmClientSettings.Add(settignsItem);
            }
        }
        else
        {
            throw new ArgumentException("Invalid LLM provider", llmProviderSettings.Value);
        }

        _llmClientSettings.LlmProvider = llmProviderSettings.Value;
    }

    private void AddTranscriptionSettings()
    {
        var modelSettings = new EntrySettingsItem(_dialogService) { Title = "Model", Value = _transcriptionSettings.Model };
        modelSettings.ValueChanged += (s, e) => _transcriptionSettings.Model = modelSettings.Value;

        var apiKeySettings = new SecureSettingsItem(_dialogService, _transcriptionSettings)
        {
            Title = "Api Key",
            IsValueSetAsync = true,
            SecondaryTextMode = SecondaryTextMode.ValueMasked,
        };

        var endpointSettings = new EntrySettingsItem(_dialogService)
        {
            Title = "Endpoint",
            Value = _transcriptionSettings.Endpoint,
            Description = "Edit this field only when using self hosted whisper models",
            SecondaryTextMode = SecondaryTextMode.Description,
        };
        endpointSettings.ValueChanged += (s, e) => _transcriptionSettings.Endpoint = endpointSettings.Value;

        SettingsItems.Add(new SettingsItemsGroup("Transcription", [modelSettings, apiKeySettings, endpointSettings]));
    }

    private void AddOpenAiSettings()
    {
        var modelSettings = new EntrySettingsItem(_dialogService) { Title = "Model", Value = _openAiClientSettings.Model };
        modelSettings.ValueChanged += (s, e) => _openAiClientSettings.Model = modelSettings.Value;

        var apiKeySettings = new SecureSettingsItem(_dialogService, _openAiClientSettings)
        {
            Title = "Api Key",
            IsValueSetAsync = true,
            SecondaryTextMode = SecondaryTextMode.ValueMasked,
        };

        var endpointSettings = new EntrySettingsItem(_dialogService)
        {
            Title = "Endpoint",
            Description = "Edit this field only when using self hosted OpeAi-compatible APIs",
            SecondaryTextMode = SecondaryTextMode.Description,
            Value = _openAiClientSettings.Endpoint,
        };
        endpointSettings.ValueChanged += (s, e) => _openAiClientSettings.Endpoint = endpointSettings.Value;

        _openAiSettings.Add(modelSettings);
        _openAiSettings.Add(apiKeySettings);
        _openAiSettings.Add(endpointSettings);
    }

    private void AddGeminiSettings()
    {
        var modelSettings = new EntrySettingsItem(_dialogService) { Title = "Model", Value = _geminiClientSettings.Model };
        modelSettings.ValueChanged += (s, e) => _geminiClientSettings.Model = modelSettings.Value;

        var apiKeySettings = new SecureSettingsItem(_dialogService, _geminiClientSettings)
        {
            Title = "Api Key",
            IsValueSetAsync = true,
            SecondaryTextMode = SecondaryTextMode.ValueMasked,
        };

        _geminiSettings.Add(modelSettings);
        _geminiSettings.Add(apiKeySettings);
    }
}
