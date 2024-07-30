#if ANDROID
using Android.Content;
using SubtitlesApp.Infrastructure.Android.Services.File;
using SubtitlesApp.Platforms.Android;
#endif

namespace SubtitlesApp.Services;

public static class MediaChooser
{
#if ANDROID
    public static async Task<string?> PickVideoAsync()
    {
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
#endif
}
