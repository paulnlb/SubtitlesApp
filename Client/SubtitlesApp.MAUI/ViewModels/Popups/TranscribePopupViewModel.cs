using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Interfaces;
using UraniumUI.Dialogs;

namespace SubtitlesApp.ViewModels.Popups;

public partial class TranscribePopupViewModel(
    IPopupService popupService,
    LanguageService languageService,
    ICustomPopupService dialogService
) : ObservableObject, IQueryAttributable
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

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(MediaDuration), out var durationValue);
        query.TryGetValue(nameof(SubtitlesLanguage), out var targetLangValue);
        query.TryGetValue(nameof(FromTime), out var fromTimeValue);
        query.TryGetValue(nameof(ToTime), out var toTimeValue);

        if (durationValue is TimeSpan mediaDuration)
        {
            MediaDuration = mediaDuration;
        }
        if (targetLangValue is Language subtitlesLanguage)
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
    }

    [RelayCommand]
    public async Task ChooseSubtitlesLanguage()
    {
        var result = await dialogService.DisplayRadioButtonPromptAsync(
            "Choose language of subtitles",
            languageService.GetAllLanguages(),
            x => x.NativeName,
            languageService.GetDefaultLanguage()
        );

        if (result is not null)
        {
            SubtitlesLanguage = result;
        }
    }

    [RelayCommand]
    public async Task Save()
    {
        var transcriptionSettings = new TranscriptionSettings
        {
            SubtitlesLanguage = SubtitlesLanguage ?? languageService.GetDefaultLanguage(),
            FromTime = FromTime,
            ToTime = ToTime,
        };

        await popupService.ClosePopupAsync(Shell.Current, transcriptionSettings);
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
