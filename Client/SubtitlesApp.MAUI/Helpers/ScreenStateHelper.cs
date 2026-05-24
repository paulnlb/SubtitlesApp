using MauiPageFullScreen;

namespace SubtitlesApp.Helpers;

public static class ScreenStateHelper
{
    private static bool IsFullScreen { get; set; } = false;

    public static void FullScreen()
    {
        if (!IsFullScreen)
        {
            Controls.FullScreen();
            IsFullScreen = true;
        }
    }

    public static void RestoreScreen()
    {
        if (IsFullScreen)
        {
            Controls.RestoreScreen();
            IsFullScreen = false;
        }
    }

    public static void ForceFullScreen()
    {
        Controls.FullScreen();
        IsFullScreen = true;
    }

    public static void ForceRestoreScreen()
    {
        Controls.RestoreScreen();
        IsFullScreen = false;
    }

    public static void ChangeOrientation(bool toLandscape)
    {
        if (DeviceInfo.Current.Platform == DevicePlatform.Android)
        {
            MainActivity.Instance.ChangeOrientation(toLandscape);
        }
    }
}
