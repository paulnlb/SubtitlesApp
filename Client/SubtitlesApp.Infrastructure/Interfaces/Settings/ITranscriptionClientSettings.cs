namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface ITranscriptionClientSettings
{
    public string Model { get; set; }

    public string? Endpoint { get; set; }

    Task<string> GetApiKey();

    Task SetApiKey(string value);
}
