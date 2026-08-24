using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.Constants;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Resources.Strings;
using SubtitlesApp.Services;
using SubtitlesApp.Views;

namespace SubtitlesApp.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly string[] _mainLabelList =
    [
        MainLabelTexts.AiUseful,
        MainLabelTexts.ThisVersion,
        MainLabelTexts.Tool,
        MainLabelTexts.Features,
        MainLabelTexts.Subtitles,
        MainLabelTexts.OpenSource,
        MainLabelTexts.TranscribeTranslate,
        MainLabelTexts.TranscriptionSolved,
        MainLabelTexts.LanguageBarrier,
        MainLabelTexts.NotWrapper,
        MainLabelTexts.Nsaas,
    ];

    private readonly LocalFileManager _localFileManager;
    private readonly ICustomPopupService _popupService;

    [ObservableProperty]
    private string _mainLabelText;

    [ObservableProperty]
    private string _footerText = $"v{AppInfo.Current.VersionString} alpha. The app may crash.";

    public MainPageViewModel(LocalFileManager localFileManager, ICustomPopupService popupService)
    {
        _localFileManager = localFileManager;
        _popupService = popupService;

        MainLabelText = _mainLabelList[Random.Shared.Next(_mainLabelList.Length)];
    }

    [RelayCommand]
    public Task OpenSettings() => Shell.Current.GoToAsync(nameof(SettingsPage));

    [RelayCommand]
    public async Task OpenMediaFile()
    {
        var actions = new List<PickerItem>
        {
            new() { Title = "Remote File", Action = FileActions.LoadRemote },
            new() { Title = "Local File", Action = FileActions.LoadLocal },
        };

        var result = await _popupService.ShowActionList("Choose a Source", actions, x => x.Title);

        switch (result?.Action)
        {
            case FileActions.LoadRemote:

                var url = await _popupService.ShowUrlEntry();

                if (!string.IsNullOrEmpty(url))
                {
                    var remoteInfo = new MediaFileInfo(
                        FileResourceType.Remote,
                        url,
                        new Uri(url).Segments.Last().TrimEnd('/'),
                        url
                    );

                    await OpenPlayerWithSubtitlesPage(remoteInfo);
                }

                break;

            case FileActions.LoadLocal:

                var localInfo = await _localFileManager.PickFile([MimeTypes.AnyVideo, MimeTypes.AnyAudio]);

                if (localInfo is not null)
                {
                    await OpenPlayerWithSubtitlesPage(localInfo);
                }

                break;
        }
    }

    private static Task OpenPlayerWithSubtitlesPage(MediaFileInfo fileResource)
    {
        return Shell.Current.GoToAsync(
            nameof(PlayerWithSubtitlesPage),
            new Dictionary<string, object> { { "open", fileResource } }
        );
    }
}
