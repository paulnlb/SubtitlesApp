namespace SubtitlesServer.TranslationApi.Configs;

public class LlmTranslationConfig
{
    public LlmProvider LlmProvider { get; set; }
}

public enum LlmProvider
{
    Ollama,
    OpenAi,
}
