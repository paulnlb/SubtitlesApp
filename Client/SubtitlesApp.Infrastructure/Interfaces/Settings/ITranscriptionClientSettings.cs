namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface ITranscriptionClientSettings
{
    public string Model { get; set; }

    public string ApiKey { get; set; }

    public string? Endpoint { get; set; }
}
