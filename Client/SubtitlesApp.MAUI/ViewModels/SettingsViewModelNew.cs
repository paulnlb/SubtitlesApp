using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.Constants;
using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Interfaces;
using SubtitlesApp.ViewModels.SettingsItems;
using SubtitlesApp.Views;

namespace SubtitlesApp.ViewModels;

public partial class SettingsViewModelNew : ObservableObject
{
    private const string ValueMask = "******";

    private readonly ITranscriptionClientSettings _transcriptionClientSettings;
    private readonly IGeminiClientSettings _geminiClientSettings;
    private readonly IOpenAiClientSettings _openAiClientSettings;
    private readonly ILlmSettings _llmClientSettings;
    private readonly ICustomPopupService _popupService;

    private readonly ObservableCollection<SettingsItem> _openAiSettings = [];
    private readonly ObservableCollection<SettingsItem> _geminiSettings = [];

    public ObservableCollection<SettingsItemsGroup> SettingsItems { get; } = [];

    public SettingsViewModelNew(
        ITranscriptionClientSettings transcriptionClientSettings,
        IGeminiClientSettings geminiClientSettings,
        IOpenAiClientSettings openAiClientSettings,
        ILlmSettings llmClientSettings,
        ICustomPopupService popupService
    )
    {
        _transcriptionClientSettings = transcriptionClientSettings;
        _geminiClientSettings = geminiClientSettings;
        _openAiClientSettings = openAiClientSettings;
        _llmClientSettings = llmClientSettings;
        _popupService = popupService;

        AddTranscriptionSettings();
        AddOpenAiSettings();
        AddGeminiSettings();
        AddLlmProviderSettings();

        if (llmClientSettings.LlmProvider == LlmProviderConstants.OpenAi)
        {
            SettingsItems.Add(new SettingsItemsGroup(AppSettings.OnlineLlmTranslationGroup, [.. _openAiSettings]));
        }
        else if (llmClientSettings.LlmProvider == LlmProviderConstants.Gemini)
        {
            SettingsItems.Add(new SettingsItemsGroup(AppSettings.OnlineLlmTranslationGroup, [.. _geminiSettings]));
        }

        AddLogsPage();
    }

    private void UpdateLlmProvider(string? newValue)
    {
        var group = SettingsItems.Single(x => x.Name == AppSettings.OnlineLlmTranslationGroup);

        if (newValue == LlmProviderConstants.OpenAi)
        {
            group.Items.Clear();

            foreach (var settignsItem in _openAiSettings)
            {
                group.Items.Add(settignsItem);
            }
        }
        else if (newValue == LlmProviderConstants.Gemini)
        {
            group.Items.Clear();

            foreach (var settignsItem in _geminiSettings)
            {
                group.Items.Add(settignsItem);
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
            _popupService,
            true,
            () => _transcriptionClientSettings.Model,
            (value) => _transcriptionClientSettings.Model = value
        )
        {
            Title = "Model",
        };

        var apiKeySettings = new AsyncEntrySettingsItem(
            _popupService,
            _transcriptionClientSettings.GetSecret,
            _transcriptionClientSettings.SetSecret
        )
        {
            Title = "Api Key",
            SubTitle = ValueMask,
        };

        var endpointSettings = new EntrySettingsItem(
            _popupService,
            false,
            () => _transcriptionClientSettings.Endpoint ?? string.Empty,
            (value) => _transcriptionClientSettings.Endpoint = value
        )
        {
            Title = "Endpoint",
            SubTitle = "Set endpoint to use third-party/self-hosted OpenAi-compatible Whisper APIs",
        };

        var advancedSettings = new PageSettingsItem(nameof(TranscriptionSettingsPage)) { Title = "Advanced Settings" };

        SettingsItems.Add(
            new SettingsItemsGroup(
                AppSettings.OnlineTranscriptionGroup,
                [modelSettings, apiKeySettings, endpointSettings, advancedSettings]
            )
        );
    }

    private void AddOpenAiSettings()
    {
        var modelSettings = new EntrySettingsItem(
            _popupService,
            true,
            () => _openAiClientSettings.Model,
            (value) => _openAiClientSettings.Model = value
        )
        {
            Title = "Model",
        };

        var apiKeySettings = new AsyncEntrySettingsItem(
            _popupService,
            _openAiClientSettings.GetSecret,
            _openAiClientSettings.SetSecret
        )
        {
            Title = "Api Key",
            SubTitle = ValueMask,
        };

        var endpointSettings = new EntrySettingsItem(
            _popupService,
            false,
            () => _openAiClientSettings.Endpoint ?? string.Empty,
            (value) => _openAiClientSettings.Endpoint = value
        )
        {
            Title = "Endpoint",
            SubTitle = "Set endpoint to use third-party/self-hosted OpenAi-compatible LLM APIs",
        };

        _openAiSettings.Add(modelSettings);
        _openAiSettings.Add(apiKeySettings);
        _openAiSettings.Add(endpointSettings);
    }

    private void AddGeminiSettings()
    {
        var modelSettings = new EntrySettingsItem(
            _popupService,
            true,
            () => _geminiClientSettings.Model,
            (value) => _geminiClientSettings.Model = value
        )
        {
            Title = "Model",
        };

        var apiKeySettings = new AsyncEntrySettingsItem(
            _popupService,
            _geminiClientSettings.GetSecret,
            _geminiClientSettings.SetSecret
        )
        {
            Title = "Api Key",
            SubTitle = ValueMask,
        };

        _geminiSettings.Add(modelSettings);
        _geminiSettings.Add(apiKeySettings);
    }

    private void AddLogsPage()
    {
        var logsPage = new PageSettingsItem(nameof(LogsPage)) { Title = "Open App Logs" };

        SettingsItems.Add(new SettingsItemsGroup(AppSettings.LogsGroup, [logsPage]));
    }

    private void AddLlmProviderSettings()
    {
        var llmProviderSettings = new PickerSettingsItem(
            _popupService,
            true,
            () => _llmClientSettings.LlmProvider,
            UpdateLlmProvider
        )
        {
            Title = "LLM Provider",
            AllValues = [LlmProviderConstants.OpenAi, LlmProviderConstants.Gemini],
        };

        SettingsItems.Add(new SettingsItemsGroup(AppSettings.LlmTranslationGroup, [llmProviderSettings]));
    }
}
