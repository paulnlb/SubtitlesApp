using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Interfaces;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private const string LoadOnlineVideo = "Load Online Video";
    private const string LoadLocalResource = "Choose Local Video From Device";

    private readonly IBuiltInDialogService _dialogService;
    private readonly IVideoPicker _videoPicker;
    private readonly IPopupService _popupService;

    [ObservableProperty]
    private bool _isLoggedIn;

    public MainPageViewModel(IBuiltInDialogService dialogService, IVideoPicker videoPicker, IPopupService popupService)
    {
        _dialogService = dialogService;
        _videoPicker = videoPicker;
        _popupService = popupService;

        _isLoggedIn = false;
    }

    [RelayCommand]
    public void OpenSettings() => Shell.Current.GoToAsync($"settings");

    [RelayCommand]
    public async Task OpenMediaFile()
    {
        var result = await _dialogService.DisplayActionSheet(
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
                    await OpenPlayerWithSubtitlesPage(stringPath);
                }

                break;

            case LoadLocalResource:

                var path = await _videoPicker.PickAsync();

                if (!string.IsNullOrEmpty(path))
                {
                    await OpenPlayerWithSubtitlesPage(path);
                }

                break;
        }
    }

    private static Task OpenPlayerWithSubtitlesPage(string path)
    {
        return Shell.Current.GoToAsync($"PlayerWithSubtitlesPage?open={path}");
    }
}
