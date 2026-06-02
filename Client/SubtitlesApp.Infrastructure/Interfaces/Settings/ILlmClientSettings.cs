namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface ILlmClientSettings
{
    public string LlmProvider { get; set; }

    public string OpenAiModel { get; set; }

    /// <summary>
    /// Optional. Must only be specified when using trid-party OpenAI-compatible APIs
    /// </summary>
    public string? OpenAiEndpoint { get; set; }

    Task<string> GetOpenAiApiKey();

    Task SetOpenAiApiKey(string value);

    public string GeminiModel { get; set; }

    Task<string> GetGeminiApiKey();

    Task SetGeminiApiKey(string value);
}
