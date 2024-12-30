using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public class BuiltInPopupService : IBuiltInPopupService
{
    public Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
    {
        Page page = Application.Current?.MainPage ?? throw new NullReferenceException();
        return page.DisplayActionSheet(title, cancel, destruction, buttons);
    }

    public Task DisplayAlert(string title, string message, string cancel)
    {
        Page page = Application.Current?.MainPage ?? throw new NullReferenceException();

        return page.DisplayAlert(title, message, cancel);
    }
}
