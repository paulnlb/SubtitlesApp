using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Services;
using UraniumUI.Dialogs;

namespace SubtitlesApp.ViewModels.Popups;

public partial class TranslatePopupViewModel(
    IPopupService popupService,
    LanguageService languageService,
    IDialogService dialogService
) : ObservableObject
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

    [RelayCommand]
    public async Task ChooseTargetLanguage()
    {
        var result = await dialogService.DisplayRadioButtonPromptAsync(
            "Choose language of translation",
            languageService.GetLanguages(l => l.Code != SourceLanguageCode && l.Code != "auto"),
            displayMember: "NativeName"
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

        await popupService.ClosePopupAsync(translationSettings);
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
