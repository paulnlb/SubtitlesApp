using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Infrastructure.DataModels;

public class WhisperSubtitle : Subtitle
{
    public float NoSpeechProbability { get; set; }

    public float AverageLogProbability { get; set; }

    public float CompressionRatio { get; set; }

    public override string ToString()
    {
        return $"""
            Start Time: {TimeInterval.StartTime};
            End Time: {TimeInterval.EndTime};
            Text: {Text};
            No Speech Probability: {NoSpeechProbability};
            Average Log Probability: {AverageLogProbability};
            Compression Ratio: {CompressionRatio}.
            """;
    }
}
