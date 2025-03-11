namespace SubtitlesServer.TranslationApi.Configs;

public class OpenAIConfig
{
    public string ModelName { get; set; }

    public string ApiKey { get; set; }

    /// <summary>
    /// Optional. Must be scecified only when using trid-party OpenAI-compatible APIs
    /// </summary>
    public string Endpoint { get; set; }
}
