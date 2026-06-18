namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface IGeminiClientSettings : ISecureSettings
{
    public string Model { get; set; }
}
