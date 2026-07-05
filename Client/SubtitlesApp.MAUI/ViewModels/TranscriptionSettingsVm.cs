using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Constants;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Interfaces;
using SubtitlesApp.ViewModels.SettingsItems;

namespace SubtitlesApp.ViewModels;

public class TranscriptionSettingsVm : ObservableObject
{
    private readonly ITranscriptionClientSettings _transcriptionClientSettings;
    private readonly ITranscriptionSettings _transcriptionSettings;
    private readonly ICustomPopupService _popupService;

    public ObservableCollection<SettingsItemsGroup> SettingsItems { get; } = [];

    public TranscriptionSettingsVm(
        ITranscriptionClientSettings transcriptionClientSettings,
        ITranscriptionSettings transcriptionSettings,
        ICustomPopupService customPopupService
    )
    {
        _transcriptionClientSettings = transcriptionClientSettings;
        _transcriptionSettings = transcriptionSettings;
        _popupService = customPopupService;

        AddTranscriptionChunkingSettings();
        AddThresholdSettings();
    }

    private void AddTranscriptionChunkingSettings()
    {
        var chunkLengthSettings = new TimeEntrySettingsItem(
            _popupService,
            true,
            () => _transcriptionSettings.ChunkLength,
            (value) => _transcriptionSettings.ChunkLength = value,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(10),
            TimeEntryScope.Minutes
        )
        {
            Title = "Audio chunk length",
        };

        var lastSubtitlesAsPrompt = new CounterSettingsItem(
            _popupService,
            true,
            () => _transcriptionSettings.SubtitlesAsPromptCount,
            (value) => _transcriptionSettings.SubtitlesAsPromptCount = value,
            0,
            10
        )
        {
            Title = "Use last n subtitles as prompt",
        };

        var chunkOverlapSettings = new TimeEntrySettingsItem(
            _popupService,
            true,
            () => _transcriptionSettings.OverlapSize,
            (value) => _transcriptionSettings.OverlapSize = value,
            TimeSpan.Zero,
            _transcriptionSettings.ChunkLength,
            TimeEntryScope.Seconds
        )
        {
            Title = "Chunk Overlap Size",
        };

        var epsilonSettings = new FloatEntrySettingsItem(
            _popupService,
            true,
            () => (float)_transcriptionSettings.Epsilon.TotalMilliseconds,
            (value) => _transcriptionSettings.Epsilon = TimeSpan.FromMilliseconds(value!.Value)
        )
        {
            Title = "Epsilon (milliseconds)",
        };

        SettingsItems.Add(
            new SettingsItemsGroup(
                AppSettingsConstants.TranscriptionChunkingGroup,
                [chunkLengthSettings, lastSubtitlesAsPrompt, chunkOverlapSettings, epsilonSettings]
            )
        );
    }

    private void AddThresholdSettings()
    {
        var noSpeechProbSettings = new FloatEntrySettingsItem(
            _popupService,
            true,
            () => _transcriptionClientSettings.NoSpeechProbabilityThreshold,
            (value) => _transcriptionClientSettings.NoSpeechProbabilityThreshold = value,
            0f,
            1f
        )
        {
            Title = "No Speech Probability Threshold",
        };

        var avgLogProbSettings = new FloatEntrySettingsItem(
            _popupService,
            true,
            () => _transcriptionClientSettings.AverageLogProbabilityThreshold,
            (value) => _transcriptionClientSettings.AverageLogProbabilityThreshold = value,
            max: 0f
        )
        {
            Title = "Average Log Probability Threshold",
        };

        var compesRatioSettings = new FloatEntrySettingsItem(
            _popupService,
            true,
            () => _transcriptionClientSettings.CompressionRatioThreshold,
            (value) => _transcriptionClientSettings.CompressionRatioThreshold = value,
            min: 0f
        )
        {
            Title = "Compression Ration Threshold",
        };

        SettingsItems.Add(
            new SettingsItemsGroup(
                AppSettingsConstants.WhisperSegmentThresholds,
                [noSpeechProbSettings, avgLogProbSettings, compesRatioSettings]
            )
        );
    }
}
