using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using SubtitlesApp.Constants;
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
    private FilePickerActivityCallback _filePickerActivityCallback;
    private ActivityResultLauncher? _filePickerLauncher;
    private ActivityResultLauncher? _createTextFileLauncher;
    private ActivityResultLauncher? _createSrtFileLauncher;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        RegisterForActivityResults();
    }

    public void ChangeOrientation(bool toLandscape)
    {
        RequestedOrientation = toLandscape ? ScreenOrientation.SensorLandscape : ScreenOrientation.Unspecified;
    }

    public Task<Android.Net.Uri?> LaunchTextFileSavingActivity(string fileName)
    {
        _filePickerActivityCallback.Tcs = new TaskCompletionSource<Android.Net.Uri?>();
        _createTextFileLauncher?.Launch(fileName);

        return _filePickerActivityCallback.Tcs.Task;
    }

    public Task<Android.Net.Uri?> LaunchSrtFileSavingActivity(string fileName)
    {
        _filePickerActivityCallback.Tcs = new TaskCompletionSource<Android.Net.Uri?>();
        _createSrtFileLauncher?.Launch(fileName);

        return _filePickerActivityCallback.Tcs.Task;
    }

    public Task<Android.Net.Uri?> LaunchFilePickingActivity(string[] mimeTypes)
    {
        _filePickerActivityCallback.Tcs = new TaskCompletionSource<Android.Net.Uri?>();
        _filePickerLauncher?.Launch(mimeTypes);

        return _filePickerActivityCallback.Tcs.Task;
    }

    private void RegisterForActivityResults()
    {
        _filePickerActivityCallback = new FilePickerActivityCallback();

        _filePickerLauncher = RegisterForActivityResult(
            new ActivityResultContracts.OpenDocument(),
            _filePickerActivityCallback
        );

        _createTextFileLauncher = RegisterForActivityResult(
            new ActivityResultContracts.CreateDocument(MimeTypes.PlainText),
            _filePickerActivityCallback
        );

        _createSrtFileLauncher = RegisterForActivityResult(
            new ActivityResultContracts.CreateDocument(MimeTypes.SubtitleSrt),
            _filePickerActivityCallback
        );
    }
}
