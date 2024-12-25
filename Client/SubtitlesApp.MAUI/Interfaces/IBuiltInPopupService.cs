namespace SubtitlesApp.Interfaces;

public interface IBuiltInPopupService
{
    Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons);

    Task DisplayAlert(string title, string message, string cancel);
}
