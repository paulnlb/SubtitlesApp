using SubtitlesApp.Core.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class LlmTranslationSettings : ILlmTranslationSettings
{
    public string DefaultSystemPrompt
    {
        get =>
            """
                **Role**: You are a highly skilled translator specializing in translating subtitles for media content.
                **Task**: Translate the provided subtitles accurately while preserving the original meaning, tone, and context. Ensure that the translations are culturally appropriate and maintain the intended emotional impact. Pay special attention to idiomatic expressions, humor, and cultural references to ensure they resonate with the target audience.
                """;
    }
    public int RetryCount
    {
        get => 3;
    }
}
