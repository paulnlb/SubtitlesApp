using System.Text;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Extensions;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public class BuiltInDialogService : IBuiltInDialogService
{
    private Page CurrentPage => Application.Current?.Windows[0].Page ?? throw new NullReferenceException();

    public Task<string> DisplayActionSheet(string title, string cancel, string? destruction, params string[] buttons)
    {
        return CurrentPage.DisplayActionSheetAsync(title, cancel, destruction, buttons);
    }

    public Task DisplayAlert(string title, string message, string cancel)
    {
        return CurrentPage.DisplayAlertAsync(title, message, cancel);
    }

    public Task DisplayAlert(string title, string message, string accept, string cancel)
    {
        return CurrentPage.DisplayAlertAsync(title, message, accept, cancel);
    }

    public Task DisplayError(Error error)
    {
        var errorText = new StringBuilder();

        errorText.Append(error.Code.GetBriefDescription());

        if (!string.IsNullOrEmpty(error.Description))
        {
            errorText.Append($"\n\nDetails\n{error.Description}");
        }

        return CurrentPage.DisplayAlertAsync("Error", errorText.ToString(), "OK");
    }

    public Task<string> DisplayPrompt(string title, string? message, string initialValue = "")
    {
        return CurrentPage.DisplayPromptAsync(title, message, initialValue: initialValue);
    }
}
