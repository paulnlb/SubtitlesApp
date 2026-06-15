namespace SubtitlesApp.Core.Interfaces.Settings;

public interface ITranscriptionSettings
{
    public TimeSpan ChunkLength { get; set; }
    public TimeSpan OverlapSize { get; }

    public TimeSpan Epsilon { get; }
}
