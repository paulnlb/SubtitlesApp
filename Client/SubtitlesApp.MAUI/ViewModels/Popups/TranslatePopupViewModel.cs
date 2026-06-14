using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public partial class TranslatePopupViewModel(ICustomPopupService popupService, LanguageService languageService)
    : BasePopupVm,
        IQueryAttributable
{
    [ObservableProperty]
    private Language _targetLanguage = default!;

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

    public required string SourceLanguageCode;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(MediaDuration), out var durationValue);
        query.TryGetValue(nameof(TargetLanguage), out var targetLangValue);
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
        if (targetLangValue is Language targetLanguage)
        {
            TargetLanguage = targetLanguage;
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
    public async Task ChooseTargetLanguage()
    {
        var selectedLang = await popupService.ShowRadioButtons(
            "Choose language of translation",
            languageService.GetLanguages(l => l.Code != SourceLanguageCode && l.Code != "auto"),
            x => $"{x.Name} ({x.NativeName})",
            TargetLanguage
        );

        if (selectedLang is not null)
        {
            TargetLanguage = selectedLang;
        }
    }

    public override async Task Accept()
    {
        var translationSettings = new TranslationSettings
        {
            TargetLanguage = TargetLanguage,
            FromTime = FromTime,
            ToTime = ToTime,
        };

        await popupService.CloseCurrentAsync(translationSettings);
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
