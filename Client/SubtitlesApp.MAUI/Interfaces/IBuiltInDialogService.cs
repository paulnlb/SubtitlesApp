using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface IBuiltInDialogService
{
    Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons);

    Task DisplayAlert(string title, string message, string cancel);

    Task DisplayError(Error error);
}
