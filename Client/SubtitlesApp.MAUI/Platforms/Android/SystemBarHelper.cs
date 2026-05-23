using Android.Content.Res;
using Android.Util;

namespace SubtitlesApp.Platforms.Android;

public static class SystemBarHelper
{
    /// <summary>
    /// Returns the status bar height in pixels.
    /// </summary>
    public static int GetStatusBarHeight()
    {
        int resourceId = Resources.System.GetIdentifier("status_bar_height", "dimen", "android");

        if (resourceId > 0)
            return Resources.System.GetDimensionPixelSize(resourceId);

        return 0;
    }

    /// <summary>
    /// Returns the navigation bar height in pixels.
    /// Returns 0 if gesture navigation is used or nav bar is absent.
    /// </summary>
    public static int GetNavigationBarHeight()
    {
        int resourceId = Resources.System.GetIdentifier("navigation_bar_height", "dimen", "android");

        if (resourceId > 0)
            return Resources.System.GetDimensionPixelSize(resourceId);

        return 0;
    }

    /// <summary>
    /// Returns true if the device actually has a visible navigation bar.
    /// </summary>
    private static bool HasNavigationBar()
    {
        var activity = Platform.CurrentActivity;

        if (activity == null)
            return false;

        var metrics = new DisplayMetrics();
        activity.WindowManager?.DefaultDisplay?.GetMetrics(metrics);

        int usableHeight = metrics.HeightPixels;
        int usableWidth = metrics.WidthPixels;

        var realMetrics = new DisplayMetrics();
        activity.WindowManager?.DefaultDisplay?.GetRealMetrics(realMetrics);

        int realHeight = realMetrics.HeightPixels;
        int realWidth = realMetrics.WidthPixels;

        return realWidth > usableWidth || realHeight > usableHeight;
    }

    public static double PxToDp(int px)
    {
        return px / Resources.System.DisplayMetrics.Density;
    }
}
