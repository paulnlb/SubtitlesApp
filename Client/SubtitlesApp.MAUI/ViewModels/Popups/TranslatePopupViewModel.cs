using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Services;

namespace SubtitlesApp.ViewModels.Popups;

public partial class TranslatePopupViewModel(IPopupService popupService, LanguageService languageService) : ObservableObject
{
    [ObservableProperty]
    private TranslationSettings _settings;

    [RelayCommand]
    public async Task Save()
    {
        Settings = new()
        {
            SubtitlesLanguage = languageService.GetDefaultLanguage(),
            FromTime = TimeSpan.Zero,
            ToTime = TimeSpan.FromSeconds(30),
        };

        await popupService.ClosePopupAsync(Settings);
    }

    [RelayCommand]
    public async Task Cancel()
    {
        await popupService.ClosePopupAsync();
    }
}
