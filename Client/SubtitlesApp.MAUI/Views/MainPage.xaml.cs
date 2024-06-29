namespace SubtitlesApp.Views;

public partial class MainPage : ContentPage
{
    const string loadOnlineVideo = "Load Online Video (Not implemented)";
    const string loadLocalResource = "Load Local Resource";

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
                PickOptions pickopt = new()
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            { DevicePlatform.iOS, new[] { "public.video" } }, // UTType values
                            { DevicePlatform.Android, new[] { "video/*" } }, // MIME types
                            { DevicePlatform.WinUI, new[] { "mp4", "avi" } }, // file extensions
                            { DevicePlatform.macOS, new[] { "public.video" } }, // UTType values
                            { DevicePlatform.Tizen, new[] { "video/*" } }, // MIME types
                        }),

                    PickerTitle = "Select an audio to transcribe"
                };

                var file = await FilePicker.Default.PickAsync(pickopt);

                if (file != null)
                {
                    OpenMediaElementPage(file.FullPath);
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
