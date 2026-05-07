namespace SubtitlesServer.TranslationApi.Configs;

public class LlmTranslationConfig
{
    public LlmProvider LlmProvider { get; set; }

    public string DefaultSystemPrompt { get; set; } = default!;
}

public enum LlmProvider
{
    Ollama,
    OpenAi,
}
