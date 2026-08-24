namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface ITranscriptionSettings
{
    TimeSpan ChunkLength { get; set; }

    int SubtitlesAsPromptCount { get; set; }

    TimeSpan OverlapSize { get; set; }

    TimeSpan Epsilon { get; set; }
}
