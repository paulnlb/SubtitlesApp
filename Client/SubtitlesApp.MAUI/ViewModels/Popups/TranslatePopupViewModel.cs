using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public partial class TranslatePopupViewModel(
    IPopupService popupService,
    LanguageService languageService,
    ICustomPopupService dialogService
) : ObservableObject, IQueryAttributable
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

    public required string SourceLanguageCode;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(MediaDuration), out var durationValue);
        query.TryGetValue(nameof(TargetLanguage), out var targetLangValue);
        query.TryGetValue(nameof(FromTime), out var fromTimeValue);
        query.TryGetValue(nameof(ToTime), out var toTimeValue);

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
        var result = await dialogService.DisplayRadioButtonPromptAsync(
            "Choose language of translation",
            languageService.GetLanguages(l => l.Code != SourceLanguageCode && l.Code != "auto"),
            x => x.Name == "Auto" ? x.Name : $"{x.Name} ({x.NativeName})"
        );

        if (result != null)
        {
            TargetLanguage = result;
        }
    }

    [RelayCommand]
    public async Task Save()
    {
        var translationSettings = new TranslationSettings
        {
            TargetLanguage = TargetLanguage,
            FromTime = FromTime,
            ToTime = ToTime,
        };

        await popupService.ClosePopupAsync(Shell.Current, translationSettings);
    }

    [RelayCommand]
    public async Task Cancel()
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
}
