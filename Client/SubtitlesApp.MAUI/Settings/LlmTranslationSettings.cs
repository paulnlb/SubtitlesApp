using SubtitlesApp.Core.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class LlmTranslationSettings : ILlmTranslationSettings
{
    public string DefaultSystemPrompt =>
        """
            You are a subtitle translation engine.
            Rules:
            - Translate each item independently.
            - Do not merge, split, reorder, or omit items.
            - Preserve numbering exactly.
            - Output valid JSON only.
            """;
    public int RetryCount => 3;

    public int ChunkSize => 10;

    public int Overlap => 1;
}
