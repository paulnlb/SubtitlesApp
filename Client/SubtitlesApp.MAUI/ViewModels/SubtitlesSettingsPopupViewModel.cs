using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Services;
using UraniumUI.Dialogs;

namespace SubtitlesApp.ViewModels;

public partial class SubtitlesSettingsPopupViewModel(
    IDialogService dialogService,
    LanguageService languageService,
    IPopupService popupService
) : ObservableObject
{
    [ObservableProperty]
    SubtitlesSettings _settings;

    [RelayCommand]
    public async Task ChooseOriginalLanguage()
    {
        var result = await dialogService.DisplayRadioButtonPromptAsync(
            "Choose language of subtitles",
            languageService.GetLanguages(l => l.Code != Settings.TranslateToLanguage?.Code),
            languageService.GetDefaultLanguage(),
            displayMember: "NativeName"
        );

        if (result != null)
        {
            Settings.OriginalLanguage = result;
        }
    }

    [RelayCommand]
    public async Task OpenTranslationSettings()
    {
        await popupService.ShowPopupAsync<TranslationSettingsPopupViewModel>(vm =>
        {
            vm.SubtitlesSettings = Settings;
            vm.EnableTranslation = Settings.TranslateToLanguage is not null;
        });
    }

    [RelayCommand]
    public async Task Save()
    {
        await popupService.ClosePopupAsync(Settings);
    }

    [RelayCommand]
    public async Task Cancel()
    {
        await popupService.ClosePopupAsync();
    }
}
