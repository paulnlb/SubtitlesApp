namespace SubtitlesApp.Core.Interfaces.Settings;

public interface ITranscriptionSettings
{
    public TimeSpan SubIntervalSize { get; }
    public TimeSpan OverlapSize { get; }

    public TimeSpan Epsilon { get; }
}
