using CommunityToolkit.Maui.Views;

namespace SubtitlesApp.Helpers;

public static class ViewSizeHelper
{
    public static void SetPopupSize<T>(Popup<T> popup)
    {
        popup.MinimumHeightRequest = 100;
        popup.MaximumHeightRequest = Math.Min(500, Shell.Current.Window.Height - 50);

        popup.WidthRequest = Math.Min(300, Shell.Current.Window.Width - 50);
    }

    public static void SetWidePopupSize<T>(Popup<T> popup)
    {
        popup.MinimumHeightRequest = 100;
        popup.MaximumHeightRequest = Math.Min(500, Shell.Current.Window.Height - 50);

        popup.MinimumWidthRequest = Math.Min(300, Shell.Current.Window.Width - 50);
        popup.MaximumWidthRequest = Math.Min(700, Shell.Current.Window.Width - 50);
    }
}
