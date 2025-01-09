﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Services;
using UraniumUI.Dialogs;

namespace SubtitlesApp.ViewModels;

public partial class TranslationSettingsPopupViewModel(
    IDialogService dialogService,
    LanguageService languageService) : ObservableObject
{
    [ObservableProperty]
    SubtitlesSettings _subtitlesSettings;

    [ObservableProperty]
    bool _enableTranslation;

    [RelayCommand]
    public async Task ChooseTranslationLanguage()
    {
        var languages = languageService.GetLanguages(l => l.Code != LanguageCodes.Auto && l.Code != SubtitlesSettings.OriginalLanguage.Code);

        var result = await dialogService.DisplayRadioButtonPromptAsync(
            "Choose translation language of subtitles",
            languages,
            languages[0],
            displayMember: "Name");

        if (result != null)
        {
            SubtitlesSettings.TranslateToLanguage = result;
        }
    }

    partial void OnEnableTranslationChanged(bool value)
    {
        if (!value)
        {
            SubtitlesSettings.TranslateToLanguage = null;
        }
    }
}