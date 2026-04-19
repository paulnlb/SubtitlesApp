using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;
using UraniumUI.Dialogs;

namespace SubtitlesApp.ViewModels.Popups;

public partial class TranscribePopupViewModel(
    IPopupService popupService,
    LanguageService languageService,
    IDialogService dialogService
) : ObservableObject
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

    [RelayCommand]
    public async Task ChooseSubtitlesLanguage()
    {
        var result = await dialogService.DisplayRadioButtonPromptAsync(
            "Choose language of subtitles",
            languageService.GetAllLanguages(),
            languageService.GetDefaultLanguage(),
            displayMember: "NativeName"
        );

        if (result != null)
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

        await popupService.ClosePopupAsync(transcriptionSettings);
    }

    [RelayCommand]
    public async Task Cancel()
    {
        await popupService.ClosePopupAsync();
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
