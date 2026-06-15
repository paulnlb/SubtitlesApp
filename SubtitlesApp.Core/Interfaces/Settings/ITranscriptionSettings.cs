namespace SubtitlesApp.Core.Interfaces.Settings;

public interface ITranscriptionSettings
{
    TimeSpan ChunkLength { get; set; }

    int SubtitlesAsPromptCount { get; set; }
    TimeSpan OverlapSize { get; }

    TimeSpan Epsilon { get; }
}
