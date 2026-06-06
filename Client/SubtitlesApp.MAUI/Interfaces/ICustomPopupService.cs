namespace SubtitlesApp.Interfaces;

public interface ICustomPopupService
{
    Task<T?> DisplayRadioButtonPromptAsync<T>(
        string message,
        IEnumerable<T> selectionSource,
        Func<T, string> displaySelector,
        T? selected = default,
        string accept = "Ok",
        string cancel = "Cancel"
    );
}
