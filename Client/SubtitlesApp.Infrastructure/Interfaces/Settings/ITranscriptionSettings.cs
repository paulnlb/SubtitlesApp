namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface ITranscriptionSettings
{
    public string Model { get; set; }

    public string ApiKey { get; set; }

    public string? Endpoint { get; set; }
}
