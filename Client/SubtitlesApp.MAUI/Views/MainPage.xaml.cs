using SubtitlesApp.Services;

namespace SubtitlesApp.Views;

public partial class MainPage : ContentPage
{
    const string loadOnlineVideo = "Load Online Video (Not implemented)";
    const string loadLocalResource = "Choose Local Video From Device";

    public MainPage()
    {
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
                await Permissions.RequestAsync<Permissions.StorageRead>();
                await Permissions.RequestAsync<Permissions.StorageWrite>();

                string? path = null;

#if ANDROID
                path = await MediaChooser.PickVideoAsync();
#endif

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
}
