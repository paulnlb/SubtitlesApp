namespace SubtitlesApp.Platforms.Android;

public class ReadPhotoVideoPerms : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new List<(string androidPermission, bool isRuntime)>
        {
            (global::Android.Manifest.Permission.ReadMediaImages, true),
            (global::Android.Manifest.Permission.ReadMediaVideo, true)
        }
        .ToArray();
}
