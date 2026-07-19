using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Services;
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
        "Not just another API wrapper!",
        "Video transcription is largely solved",
        "This version is newer than anything we've ever released",
    ];

    private readonly IBuiltInDialogService _dialogService;
    private readonly LocalFileManager _localFileManager;
    private readonly ICustomPopupService _popupService;

    [ObservableProperty]
    private string _mainLabelText;

    [ObservableProperty]
    private string _footerText = $"v{AppInfo.Current.VersionString} alpha. The app may crash.";

    public MainPageViewModel(
        IBuiltInDialogService dialogService,
        LocalFileManager localFileManager,
        ICustomPopupService popupService
    )
    {
        _dialogService = dialogService;
        _localFileManager = localFileManager;
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

                var url = await _popupService.ShowUrlEntry();

                if (!string.IsNullOrEmpty(url))
                {
                    await OpenPlayerWithSubtitlesPage(new RemoteFileResource(url));
                }

                break;

            case LoadLocalResource:

                var localFileResource = await _localFileManager.PickFileAsync(["video/*", "audio/*"]);

                if (localFileResource is not null)
                {
                    await OpenPlayerWithSubtitlesPage(localFileResource);
                }

                break;
        }
    }

    private static Task OpenPlayerWithSubtitlesPage(IFileResource fileResource)
    {
        return Shell.Current.GoToAsync(
            nameof(PlayerWithSubtitlesPage),
            new Dictionary<string, object> { { "open", fileResource } }
        );
    }
}
