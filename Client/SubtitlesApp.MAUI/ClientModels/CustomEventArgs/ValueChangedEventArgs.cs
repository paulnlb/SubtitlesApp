namespace SubtitlesApp.ClientModels.CustomEventArgs;

public class ValueChangedEventArgs(string? newValue) : System.EventArgs
{
    public string? NewValue { get; } = newValue;
}
