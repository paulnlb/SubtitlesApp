namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface ITranscriptionClientSettings : ISecureSettings
{
    public string Model { get; set; }

    public string? Endpoint { get; set; }

    public float? NoSpeechProbabilityThreshold { get; set; }

    public float? AverageLogProbabilityThreshold { get; set; }

    public float? CompressionRatioThreshold { get; set; }
}
