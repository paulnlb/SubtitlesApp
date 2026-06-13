using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;

namespace SubtitlesApp.ViewModels.Popups;

public partial class TranscribePopupViewModel(IPopupService popupService, LanguageService languageService)
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

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(MediaDuration), out var durationValue);
        query.TryGetValue(nameof(SubtitlesLanguage), out var sourceLangValue);
        query.TryGetValue(nameof(FromTime), out var fromTimeValue);
        query.TryGetValue(nameof(ToTime), out var toTimeValue);
        query.TryGetValue(nameof(Title), out var titleValue);
        query.TryGetValue(nameof(AcceptText), out var acceptTextValue);
        query.TryGetValue(nameof(CancelText), out var cancelTextValue);

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

        query.Clear();
    }

    [RelayCommand]
    public async Task ChooseSubtitlesLanguage()
    {
        Func<Language, string> displaySelector = x => x.Name == "Auto" ? x.Name : $"{x.Name} ({x.NativeName})";

        var queryAttributes = new Dictionary<string, object>
        {
            [nameof(SelectLanguagePopupVm.Title)] = "Choose language of subtitles",
            [nameof(SelectLanguagePopupVm.SourceItems)] = languageService.GetAllLanguages(),
            [nameof(SelectLanguagePopupVm.DisplaySelector)] = displaySelector,
            [nameof(SelectLanguagePopupVm.SelectedItem)] = SubtitlesLanguage ?? languageService.GetDefaultLanguage(),
        };

        var popupResult = await popupService.ShowPopupAsync<SelectLanguagePopupVm, Language>(
            Shell.Current,
            new PopupOptions { Shape = null, Shadow = null },
            queryAttributes
        );

        if (popupResult.Result is not null)
        {
            SubtitlesLanguage = popupResult.Result;
        }
    }

    public override async Task Accept()
    {
        var transcriptionSettings = new TranscriptionSettings
        {
            SubtitlesLanguage = this.SubtitlesLanguage ?? languageService.GetDefaultLanguage(),
            FromTime = FromTime,
            ToTime = ToTime,
        };

        await popupService.ClosePopupAsync(Shell.Current, transcriptionSettings);
    }

    public override async Task Cancel()
    {
        await popupService.ClosePopupAsync(Shell.Current);
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
