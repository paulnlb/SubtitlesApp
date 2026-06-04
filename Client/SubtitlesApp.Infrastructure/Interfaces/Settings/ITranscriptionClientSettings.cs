namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface ITranscriptionClientSettings : ISecureSettings
{
    public string Model { get; set; }

    public string? Endpoint { get; set; }
}
