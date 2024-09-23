using CommunityToolkit.Maui.Views;
using SubtitlesApp.Maui.Interfaces;
using SubtitlesApp.CustomControls;
using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class MainPage : ContentPage
{
    const string loadOnlineVideo = "Load Online Video";
    const string loadLocalResource = "Choose Local Video From Device";

    readonly IVideoPicker _videoPicker;

    public MainPage(IVideoPicker videoPicker, MainPageViewModel vm)
    {
        _videoPicker = videoPicker;

        BindingContext = vm;

        InitializeComponent();
    }

    async void ChangeSourceClicked(object sender, EventArgs e)
    {
        var result = await DisplayActionSheet("Choose a source", "Cancel", null,
            loadOnlineVideo, loadLocalResource);

        switch (result)
        {
            case loadOnlineVideo:
                var popup = new InputPopup();
                var popupResult = await this.ShowPopupAsync(popup, CancellationToken.None);

                if (popupResult is string stringPath && !string.IsNullOrEmpty(stringPath))
                {
                    OpenMediaElementPage(stringPath);
                }

                break;

            case loadLocalResource:

                var permissionsGranded = await RequestPermissions();

                if (!permissionsGranded)
                {
                    return;
                }

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
            Shell.Current.GoToAsync($"{nameof(MediaElementPage)}?open={path}");
    }

    async Task<bool> RequestPermissions()
    {
        var readPermissionStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
        var writePermissionStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

        if (readPermissionStatus != PermissionStatus.Granted || writePermissionStatus != PermissionStatus.Granted)
        {
            await DisplayAlert(
                "Permission Denied",
                "You must grant permission to read and write to storage. Please turn on permissions in settings.",
                "Got it");

            return false;
        }

        return true;
    }
}
