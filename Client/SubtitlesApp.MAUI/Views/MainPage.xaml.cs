using SubtitlesApp.Application.Interfaces;

namespace SubtitlesApp.Views;

public partial class MainPage : ContentPage
{
    const string loadOnlineVideo = "Load Online Video (Not implemented)";
    const string loadLocalResource = "Choose Local Video From Device";

    readonly IVideoPicker _videoPicker;

    public MainPage(IVideoPicker videoPicker)
    {
        _videoPicker = videoPicker;

        InitializeComponent();
    }

    async void ChangeSourceClicked(object sender, EventArgs e)
    {
        var result = await DisplayActionSheet("Choose a source", "Cancel", null,
            loadOnlineVideo, loadLocalResource);

        switch (result)
        {
            case loadOnlineVideo:
                return;

            case loadLocalResource:

                var permissionsGranded = await RequestPermissions();

                if (!permissionsGranded)
                {
                    return;
                }

                var path = await _videoPicker.PickAsync();

                if (path != null)
                {
                    OpenMediaElementPage(path);
                }

                return;
        }
    }

    void OpenMediaElementPage(string path)
    {
        if (path != null || path != string.Empty)
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
