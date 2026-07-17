using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using SubtitlesApp.Services;

namespace SubtitlesApp;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTask,
    ResizeableActivity = true,
    ConfigurationChanges = ConfigChanges.ScreenSize
        | ConfigChanges.Orientation
        | ConfigChanges.UiMode
        | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize
        | ConfigChanges.Density
)]
public class MainActivity : MauiAppCompatActivity
{
    internal static MainActivity Instance { get; private set; }
    public ActivityResultLauncher? FilePickerLauncher { get; private set; }
    public FilePickerActivityCallback FilePickerActivityCallback { get; private set; }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Instance = this;

        FilePickerActivityCallback = new FilePickerActivityCallback();

        var activity = (AndroidX.Activity.ComponentActivity)Platform.CurrentActivity!;

        FilePickerLauncher = activity.RegisterForActivityResult(
            new ActivityResultContracts.OpenDocument(),
            FilePickerActivityCallback
        );
    }

    public void ChangeOrientation(bool toLandscape)
    {
        RequestedOrientation = toLandscape ? ScreenOrientation.SensorLandscape : ScreenOrientation.Unspecified;
    }
}
