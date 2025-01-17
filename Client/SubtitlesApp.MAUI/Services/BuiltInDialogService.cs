using System.Text;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Extensions;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public class BuiltInDialogService : IBuiltInDialogService
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

    public Task DisplayError(Error error)
    {
        Page page = Application.Current?.MainPage ?? throw new NullReferenceException();

        var errorText = new StringBuilder();

        errorText.Append(error.Code.GetBriefDescription());

        if (!string.IsNullOrEmpty(error.Description))
        {
            errorText.Append(
                $@"

Details:
{error.Description}"
            );
        }

        return page.DisplayAlert("Error", errorText.ToString(), "OK");
    }
}
