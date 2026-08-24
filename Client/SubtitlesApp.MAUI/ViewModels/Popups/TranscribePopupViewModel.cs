using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public partial class TranscribePopupViewModel(ICustomPopupService popupService, LanguageService languageService)
    : BasePopupVm,
        IQueryAttributable
{
    [ObservableProperty]
    private Language? _subtitlesLanguage;

    [ObservableProperty]
    private TimeSpan _fromTime;

    [ObservableProperty]
    private TimeSpan _toTime;

    [ObservableProperty]
    private TimeSpan _mediaDuration;

    [ObservableProperty]
    private TimeSpan _currentMediaTime;

    [ObservableProperty]
    private ObservableCollection<TimePreset> _timePresets = [];

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(MediaDuration), out var durationValue);
        query.TryGetValue(nameof(SubtitlesLanguage), out var sourceLangValue);
        query.TryGetValue(nameof(FromTime), out var fromTimeValue);
        query.TryGetValue(nameof(ToTime), out var toTimeValue);
        query.TryGetValue(nameof(Title), out var titleValue);
        query.TryGetValue(nameof(AcceptText), out var acceptTextValue);
        query.TryGetValue(nameof(CancelText), out var cancelTextValue);
        query.TryGetValue(nameof(CurrentMediaTime), out var currentMediaTimeValue);

        if (titleValue is string title)
        {
            Title = title;
        }
        if (acceptTextValue is string acceptText)
        {
            AcceptText = acceptText;
        }
        if (cancelTextValue is string cancelText)
        {
            CancelText = cancelText;
        }
        if (durationValue is TimeSpan mediaDuration)
        {
            MediaDuration = mediaDuration;
        }
        if (sourceLangValue is Language subtitlesLanguage)
        {
            SubtitlesLanguage = subtitlesLanguage;
        }
        if (fromTimeValue is TimeSpan fromTime)
        {
            FromTime = fromTime;
        }
        if (toTimeValue is TimeSpan toTime)
        {
            ToTime = toTime;
        }
        if (currentMediaTimeValue is TimeSpan currentMediaTime)
        {
            CurrentMediaTime = currentMediaTime;
        }

        AddTimePresets();

        query.Clear();
    }

    [RelayCommand]
    public async Task ChooseSubtitlesLanguage()
    {
        var selectedLang = await popupService.ShowRadioButtons(
            "Choose language of subtitles",
            languageService.GetAllLanguages(),
            x => x.Name == "Auto" ? x.Name : $"{x.Name} ({x.NativeName})",
            SubtitlesLanguage ?? languageService.GetDefaultLanguage()
        );

        if (selectedLang is not null)
        {
            SubtitlesLanguage = selectedLang;
        }
    }

    [RelayCommand]
    public async Task EnterFromTime()
    {
        var result = await popupService.ShowTimeEntry(
            "Transcription Start Time",
            FromTime,
            TimeSpan.Zero,
            ToTime,
            timePresets: TimePresets
        );

        if (result is TimeSpan selectedTime)
        {
            FromTime = selectedTime;
        }
    }

    [RelayCommand]
    public async Task EnterToTime()
    {
        var result = await popupService.ShowTimeEntry(
            "Transcription End Time",
            ToTime,
            FromTime,
            MediaDuration,
            timePresets: TimePresets
        );

        if (result is TimeSpan selectedTime)
        {
            ToTime = selectedTime;
        }
    }

    public override async Task Accept()
    {
        var transcriptionSettings = new TranscriptionSettings
        {
            SubtitlesLanguage = SubtitlesLanguage ?? languageService.GetDefaultLanguage(),
            FromTime = FromTime,
            ToTime = ToTime,
        };

        await popupService.CloseCurrentAsync(transcriptionSettings);
    }

    public override async Task Cancel()
    {
        await popupService.CloseCurrentAsync();
    }

    private void AddTimePresets()
    {
        TimePresets.Add(
            new()
            {
                Type = TimePresetType.Absolute,
                Time = CurrentMediaTime,
                Title = "Current",
            }
        );
        TimePresets.Add(
            new()
            {
                Type = TimePresetType.Absolute,
                Time = TimeSpan.Zero,
                Title = "Start",
            }
        );
        TimePresets.Add(
            new()
            {
                Type = TimePresetType.Absolute,
                Time = MediaDuration,
                Title = "End",
            }
        );
        TimePresets.Add(
            new()
            {
                Type = TimePresetType.Incremental,
                Time = TimeSpan.FromSeconds(5),
                Title = "+5s",
            }
        );
        TimePresets.Add(
            new()
            {
                Type = TimePresetType.Incremental,
                Time = TimeSpan.FromSeconds(30),
                Title = "+30s",
            }
        );
        TimePresets.Add(
            new()
            {
                Type = TimePresetType.Incremental,
                Time = TimeSpan.FromMinutes(5),
                Title = "+5m",
            }
        );
        TimePresets.Add(
            new()
            {
                Type = TimePresetType.Incremental,
                Time = TimeSpan.FromMinutes(30),
                Title = "+30m",
            }
        );
    }

    partial void OnFromTimeChanged(TimeSpan value)
    {
        IsAcceptEnabled = value < ToTime;
    }

    partial void OnToTimeChanged(TimeSpan value)
    {
        IsAcceptEnabled = FromTime < value && value <= MediaDuration;
    }

    partial void OnMediaDurationChanged(TimeSpan value)
    {
        FromTime = TimeSpan.Zero;
        ToTime = value;
    }
}
