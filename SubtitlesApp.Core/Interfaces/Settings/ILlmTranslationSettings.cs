namespace SubtitlesApp.Core.Interfaces.Settings;

public interface ILlmTranslationSettings
{
    public string DefaultSystemPrompt { get; }

    public int RetryCount { get; }
}
