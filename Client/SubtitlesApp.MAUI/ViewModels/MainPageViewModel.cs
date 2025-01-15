using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Interfaces;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    const string LoadOnlineVideo = "Load Online Video";
    const string LoadLocalResource = "Choose Local Video From Device";

    readonly IAuthService _authService;
    readonly IBuiltInPopupService _builtInPopupService;
    readonly IVideoPicker _videoPicker;
    readonly IPopupService _popupService;

    [ObservableProperty]
    bool _isLoggedIn;

    public MainPageViewModel(
        IAuthService authService,
        IBuiltInPopupService builtInPopupService,
        IVideoPicker videoPicker,
        IPopupService popupService
    )
    {
        _authService = authService;
        _builtInPopupService = builtInPopupService;
        _videoPicker = videoPicker;
        _popupService = popupService;

        _isLoggedIn = !string.IsNullOrEmpty(_authService.GetAccessTokenAsync().Result);
    }

    [RelayCommand]
    public async Task LogInAsync()
    {
        _popupService.ShowPopup<LoadingPopupViewModel>();

        var result = await _authService.LogInAsync();

        if (result.IsSuccess)
        {
            IsLoggedIn = true;
        }
        _popupService.ClosePopup();
    }

    [RelayCommand]
    public async Task LogOutAsync()
    {
        _popupService.ShowPopup<LoadingPopupViewModel>();

        var result = await _authService.LogOutAsync();

        if (result.IsSuccess)
        {
            IsLoggedIn = false;
        }

        _popupService.ClosePopup();
    }

    [RelayCommand]
    public void OpenSettings() => Shell.Current.GoToAsync($"settings");

    [RelayCommand]
    public async Task OpenMediaFile()
    {
        var result = await _builtInPopupService.DisplayActionSheet(
            "Choose a source",
            "Cancel",
            null,
            LoadOnlineVideo,
            LoadLocalResource
        );

        switch (result)
        {
            case LoadOnlineVideo:
                var popupResult = await _popupService.ShowPopupAsync<InputPopupViewModel>();

                if (popupResult is string stringPath && !string.IsNullOrEmpty(stringPath))
                {
                    OpenPlayerWithSubtitlesPage(stringPath);
                }

                break;

            case LoadLocalResource:
                _popupService.ShowPopup<LoadingPopupViewModel>();

                var path = await _videoPicker.PickAsync();

                if (!string.IsNullOrEmpty(path))
                {
                    OpenPlayerWithSubtitlesPage(path);
                }

                _popupService.ClosePopup();

                break;
        }
    }

    void OpenPlayerWithSubtitlesPage(string path)
    {
        if (!string.IsNullOrEmpty(path))
            Shell.Current.GoToAsync($"PlayerWithSubtitlesPage?open={path}");
    }
}
