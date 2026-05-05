namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface IOpenAiSettings
{
    public string Model { get; set; }

    /// <summary>
    /// Optional. Must only be specified when using trid-party OpenAI-compatible APIs
    /// </summary>
    public string? Endpoint { get; set; }

    Task<string> GetApiKey();

    Task SetApiKey(string value);
}
