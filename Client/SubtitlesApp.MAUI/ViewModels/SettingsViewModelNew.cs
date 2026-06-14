using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.Constants;
using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Interfaces;
using SubtitlesApp.ViewModels.SettingsItems;

namespace SubtitlesApp.ViewModels;

public partial class SettingsViewModelNew : ObservableObject
{
    private const string ValueMask = "******";

    private readonly ITranscriptionClientSettings _transcriptionSettings;
    private readonly IGeminiClientSettings _geminiClientSettings;
    private readonly IOpenAiClientSettings _openAiClientSettings;
    private readonly ILlmSettings _llmClientSettings;
    private readonly ICustomPopupService _popupService;

    private readonly ObservableCollection<SettingsItem> _openAiSettings = [];
    private readonly ObservableCollection<SettingsItem> _geminiSettings = [];

    public ObservableCollection<SettingsItemsGroup> SettingsItems { get; } = [];

    public SettingsViewModelNew(
        ITranscriptionClientSettings transcriptionSettings,
        IGeminiClientSettings geminiClientSettings,
        IOpenAiClientSettings openAiClientSettings,
        ILlmSettings llmClientSettings,
        ICustomPopupService popupService
    )
    {
        _transcriptionSettings = transcriptionSettings;
        _geminiClientSettings = geminiClientSettings;
        _openAiClientSettings = openAiClientSettings;
        _llmClientSettings = llmClientSettings;
        _popupService = popupService;

        AddTranscriptionSettings();
        AddOpenAiSettings();
        AddGeminiSettings();

        var llmProviderSettings = new PickerSettingsItem(
            popupService,
            true,
            () => llmClientSettings.LlmProvider,
            UpdateLlmProvider
        )
        {
            Title = "LLM Provider",
            AllValues = [LlmProviderConstants.OpenAi, LlmProviderConstants.Gemini],
        };

        SettingsItems.Add(new SettingsItemsGroup(AppSettingsConstants.LlmTranslationGroup, [llmProviderSettings]));

        if (llmClientSettings.LlmProvider == LlmProviderConstants.OpenAi)
        {
            SettingsItems.Add(new SettingsItemsGroup(AppSettingsConstants.OnlineLlmTranslationGroup, [.. _openAiSettings]));
        }
        else if (llmClientSettings.LlmProvider == LlmProviderConstants.Gemini)
        {
            SettingsItems.Add(new SettingsItemsGroup(AppSettingsConstants.OnlineLlmTranslationGroup, [.. _geminiSettings]));
        }
    }

    private void UpdateLlmProvider(string? newValue)
    {
        var group = SettingsItems.Single(x => x.Name == AppSettingsConstants.OnlineLlmTranslationGroup);

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
            () => _transcriptionSettings.Model,
            (value) => _transcriptionSettings.Model = value
        )
        {
            Title = "Model",
        };

        var apiKeySettings = new AsyncEntrySettingsItem(
            _popupService,
            _transcriptionSettings.GetSecret,
            _transcriptionSettings.SetSecret
        )
        {
            Title = "Api Key",
            SubTitle = ValueMask,
        };

        var endpointSettings = new EntrySettingsItem(
            _popupService,
            false,
            () => _transcriptionSettings.Endpoint ?? string.Empty,
            (value) => _transcriptionSettings.Endpoint = value
        )
        {
            Title = "Endpoint",
            SubTitle = "Set endpoint to use third-party/self-hosted whisper models",
        };

        SettingsItems.Add(
            new SettingsItemsGroup(
                AppSettingsConstants.OnlineTranscriptionGroup,
                [modelSettings, apiKeySettings, endpointSettings]
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
            SubTitle = "Set endpoint to use third-party/self-hosted OpeAi-compatible APIs",
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
}
