using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Interfaces;

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

    [ObservableProperty]
    bool _isLoggedOut;

    public MainPageViewModel(
        IAuthService authService,
        IBuiltInPopupService builtInPopupService,
        IVideoPicker videoPicker,
        IPopupService popupService)
    {
        _authService = authService;
        _builtInPopupService = builtInPopupService;
        _videoPicker = videoPicker;
        _popupService = popupService;

        _isLoggedOut = string.IsNullOrEmpty(_authService.GetAccessTokenAsync(false).Result);
        _isLoggedIn = !_isLoggedOut;
    }

    [RelayCommand]
    public async Task LogInAsync()
    {
        var result = await _authService.LogInAsync();
        if (result.IsSuccess)
        {
            IsLoggedIn = true;
            IsLoggedOut = false;
        }
    }

    [RelayCommand]
    public async Task LogOutAsync()
    {
        var result = await _authService.LogOutAsync();
        if (result.IsSuccess)
        {
            IsLoggedIn = false;
            IsLoggedOut = true;
        }
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
            LoadLocalResource);


        switch (result)
        {
            case LoadOnlineVideo:
                var popupResult = await _popupService.ShowPopupAsync<InputPopupViewModel>();

                if (popupResult is string stringPath && !string.IsNullOrEmpty(stringPath))
                {
                    OpenMediaElementPage(stringPath);
                }

                break;

            case LoadLocalResource:
                var path = await _videoPicker.PickAsync();

                if (!string.IsNullOrEmpty(path))
                {
                    OpenMediaElementPage(path);
                }

                break;
        }
    }

    void OpenMediaElementPage(string path)
    {
        if (!string.IsNullOrEmpty(path))
            Shell.Current.GoToAsync($"MediaElementPage?open={path}");
    }
}
