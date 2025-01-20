namespace SubtitlesApp.Platforms.Android;

public class ReadPhotoVideoPerms : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions
    {
        get
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                return
                [
                    (global::Android.Manifest.Permission.ReadMediaImages, true),
                    (global::Android.Manifest.Permission.ReadMediaVideo, true),
                ];
            }
            else
            {
                return [(global::Android.Manifest.Permission.ReadExternalStorage, true)];
            }
        }
    }
}
