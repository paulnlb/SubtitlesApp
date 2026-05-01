namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface IOpenAiSettings
{
    public string Model { get; set; }

    public string ApiKey { get; set; }

    /// <summary>
    /// Optional. Must only be specified when using trid-party OpenAI-compatible APIs
    /// </summary>
    public string? Endpoint { get; set; }
}
