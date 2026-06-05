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
            - If a context is provided (which is the last source items from the previous run), use it to increase translation accuracy.
            - Never include the context into translations.
            - If no context provided, just translate the source items.
            """;
    public int RetryCount => 3;

    public int ChunkSize => 10;

    public int Overlap => 1;
}
