using CommunityToolkit.Maui.Views;

namespace SubtitlesApp.Helpers;

public static class ViewSizeHelper
{
    public static void SetPopupSize<T>(Popup<T> popup)
    {
        popup.MinimumHeightRequest = popup.MinimumWidthRequest = 100;
        popup.MaximumHeightRequest = Math.Min(500, Shell.Current.CurrentPage.Height - 100);
        popup.MaximumWidthRequest = Math.Min(400, Shell.Current.CurrentPage.Width - 50);
    }
}
