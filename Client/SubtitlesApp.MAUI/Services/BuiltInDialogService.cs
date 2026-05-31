using System.Text;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Extensions;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public class BuiltInDialogService : IBuiltInDialogService
{
    public Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
    {
        Page page = Application.Current?.Windows[0].Page ?? throw new NullReferenceException();
        return page.DisplayActionSheetAsync(title, cancel, destruction, buttons);
    }

    public Task DisplayAlert(string title, string message, string cancel)
    {
        Page page = Application.Current?.Windows[0].Page ?? throw new NullReferenceException();

        return page.DisplayAlertAsync(title, message, cancel);
    }

    public Task DisplayAlert(string title, string message, string accept, string cancel)
    {
        Page page = Application.Current?.Windows[0].Page ?? throw new NullReferenceException();

        return page.DisplayAlertAsync(title, message, accept, cancel);
    }

    public Task DisplayError(Error error)
    {
        Page page = Application.Current?.Windows[0].Page ?? throw new NullReferenceException();

        var errorText = new StringBuilder();

        errorText.Append(error.Code.GetBriefDescription());

        if (!string.IsNullOrEmpty(error.Description))
        {
            errorText.Append($"\n\nDetails\n{error.Description}");
        }

        return page.DisplayAlertAsync("Error", errorText.ToString(), "OK");
    }
}
