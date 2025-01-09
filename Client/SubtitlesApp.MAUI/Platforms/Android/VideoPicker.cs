using Android.Content;
using SubtitlesApp.Platforms.Android;

namespace SubtitlesApp.Services;

/// <summary>
///  Reimplementation of the VideoPicker class to avoid copying to cache
///  https://github.com/dotnet/maui/issues/6015
/// </summary>
public partial class VideoPicker
{
    public partial async Task<string?> PickAsync()
    {
        var status = await Permissions.RequestAsync<ReadPhotoVideoPerms>();

        if (status != PermissionStatus.Granted)
        {
            AppInfo.Current.ShowSettingsUI();
        }

        var currentActivity = Platform.CurrentActivity ?? await Platform.WaitForActivityAsync();

        // Essentials supports >= API 19 where this action is available
        var action = Intent.ActionOpenDocument;

        var intent = new Intent(action);
        intent.SetType("video/*");
        intent.PutExtra(Intent.ExtraAllowMultiple, false);

        try
        {
            string? result = null;

            void OnResult(Intent intent)
            {
                // The uri returned is only temporary and only lives as long as the Activity that requested it,
                // so this means that it will always be cleaned up by the time we need it because we are using
                // an intermediate activity.

                if (intent.Data != null)
                {
                    var path = FilesHelper.GetActualPathForFile(intent.Data, currentActivity);
                    result = path;
                }
            }

            await IntermediateActivity.StartAsync(intent, 1, onResult: OnResult);

            return result;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}
