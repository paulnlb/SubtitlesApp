using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
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
    private bool _isTimeRangeValid;

    [ObservableProperty]
    private TimeSpan _mediaDuration;

    [ObservableProperty]
    private bool _isStartTimeValid;

    [ObservableProperty]
    private bool _isEndTimeValid;

    [ObservableProperty]
    private TimeSpan _currentMediaTime;

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

    public void SetFromTime(TimeSpan time) => FromTime = TimeSpan.FromTicks(Math.Clamp(time.Ticks, 0, ToTime.Ticks));

    public void SetToTime(TimeSpan time) =>
        ToTime = TimeSpan.FromTicks(Math.Clamp(time.Ticks, FromTime.Ticks, MediaDuration.Ticks));

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

    partial void OnFromTimeChanged(TimeSpan value)
    {
        IsTimeRangeValid = value < ToTime;
    }

    partial void OnToTimeChanged(TimeSpan value)
    {
        IsTimeRangeValid = FromTime < value && value <= MediaDuration;
    }

    partial void OnMediaDurationChanged(TimeSpan value)
    {
        FromTime = TimeSpan.Zero;
        ToTime = value;
    }

    partial void OnIsEndTimeValidChanged(bool value)
    {
        IsAcceptEnabled = value && IsStartTimeValid && IsTimeRangeValid;
    }

    partial void OnIsStartTimeValidChanged(bool value)
    {
        IsAcceptEnabled = value && IsEndTimeValid && IsTimeRangeValid;
    }

    partial void OnIsTimeRangeValidChanged(bool value)
    {
        IsAcceptEnabled = IsStartTimeValid && IsEndTimeValid && value;
    }
}
