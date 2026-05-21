using SubtitlesApp.Core.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class TranscriptionSettings : ITranscriptionSettings
{
    public TimeSpan SubIntervalSize => TimeSpan.FromSeconds(60);

    public TimeSpan OverlapSize => TimeSpan.FromSeconds(2);

    public TimeSpan Epsilon => TimeSpan.FromMilliseconds(300);
}
