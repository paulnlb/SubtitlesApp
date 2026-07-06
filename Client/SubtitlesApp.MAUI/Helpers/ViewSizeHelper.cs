using CommunityToolkit.Maui.Views;

namespace SubtitlesApp.Helpers;

public static class ViewSizeHelper
{
    public static void SetPopupSize<T>(Popup<T> popup)
    {
        popup.MinimumHeightRequest = popup.MinimumWidthRequest = 100;
        popup.MaximumHeightRequest = Math.Min(500, Shell.Current.CurrentPage.Height - 50);
        popup.MaximumWidthRequest = Math.Min(400, Shell.Current.CurrentPage.Width - 100);
    }

    public static void SetWidePopupSize<T>(Popup<T> popup)
    {
        popup.MinimumHeightRequest = popup.MinimumWidthRequest = 100;
        popup.MaximumHeightRequest = Math.Min(500, Shell.Current.CurrentPage.Height - 50);
        popup.MaximumWidthRequest = Math.Min(700, Shell.Current.CurrentPage.Width - 100);
    }
}
