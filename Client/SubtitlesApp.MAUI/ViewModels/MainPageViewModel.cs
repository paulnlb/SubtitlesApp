using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Interfaces;
using SubtitlesApp.ViewModels.Popups;
using SubtitlesApp.Views;

namespace SubtitlesApp.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private const string LoadOnlineVideo = "Load Online Video";
    private const string LoadLocalResource = "Choose Local Video From Device";
    private readonly List<string> _mainLabelList =
    [
        "Rare case when AI is actually useful",
        "Language barrier is not a thing anymore",
        "Transcribe and translate any video",
        "Nothing impressive. Just subtitles that are less distracting",
        "Great tool for learning foreign languages",
        "Nothing special as a service",
        "The app is free and open source. The APIs - not necessarily",
        "Transcribe, translate, swipe in, swipe away, scroll and navigate",
    ];

    private readonly IBuiltInDialogService _dialogService;
    private readonly IVideoPicker _videoPicker;
    private readonly IPopupService _popupService;

    [ObservableProperty]
    private string _mainLabelText;

    [ObservableProperty]
    private string _footerText = $"v.{AppInfo.Current.VersionString}. The app may crash.";

    public MainPageViewModel(IBuiltInDialogService dialogService, IVideoPicker videoPicker, IPopupService popupService)
    {
        _dialogService = dialogService;
        _videoPicker = videoPicker;
        _popupService = popupService;

        var random = new Random();
        var index = random.Next(_mainLabelList.Count);

        MainLabelText = _mainLabelList[index];
    }

    [RelayCommand]
    public void OpenSettings() => Shell.Current.GoToAsync(nameof(SettingsPage));

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

                var stringPathResult = await _popupService.ShowPopupAsync<InputPopupViewModel, string>(
                    Shell.Current,
                    new PopupOptions { Shape = null, Shadow = null }
                );

                if (!string.IsNullOrEmpty(stringPathResult.Result))
                {
                    await OpenPlayerWithSubtitlesPage(stringPathResult.Result);
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
        return Shell.Current.GoToAsync($"{nameof(PlayerWithSubtitlesPage)}?open={path}");
    }
}
