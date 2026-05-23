namespace SubtitlesApp.Helpers;

public static class SystemBarHelper
{
    /// <summary>
    /// Returns the status bar height in pixels.
    /// </summary>
    public static double GetStatusBarHeight()
    {
#if ANDROID
        return Platforms.Android.SystemBarHelper.PxToDp(Platforms.Android.SystemBarHelper.GetStatusBarHeight());
#endif

        return 0;
    }

    /// <summary>
    /// Returns the navigation bar height in pixels.
    /// Returns 0 if gesture navigation is used or nav bar is absent.
    /// </summary>
    public static double GetNavigationBarHeight()
    {
#if ANDROID
        return Platforms.Android.SystemBarHelper.PxToDp(Platforms.Android.SystemBarHelper.GetNavigationBarHeight());
#endif

        return 0;
    }
}
