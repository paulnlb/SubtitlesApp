using MauiPageFullScreen;

namespace SubtitlesApp.Services;

public static class FullScreenWrapper
{
    private static bool _isFullScreen = false;

    public static void SetFullScreen()
    {
        if (!_isFullScreen)
        {
            Controls.FullScreen();

            _isFullScreen = true;
        }
    }

    public static void RestoreScreen()
    {
        if (_isFullScreen)
        {
            Controls.RestoreScreen();

            _isFullScreen = false;
        }
    }
}
