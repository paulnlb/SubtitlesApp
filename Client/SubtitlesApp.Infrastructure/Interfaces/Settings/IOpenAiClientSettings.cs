namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface IOpenAiClientSettings : ISecureSettings
{
    public string Model { get; set; }

    public string? Endpoint { get; set; }
}
