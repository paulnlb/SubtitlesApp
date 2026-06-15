using SubtitlesApp.Core.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class TranscriptionSettings : ITranscriptionSettings
{
    private const string _chunkLengthKey = "chunk_length_key";
    private const double _chunkLengthDefault = 30;

    public TimeSpan ChunkLength
    {
        get => TimeSpan.FromSeconds(Preferences.Get(_chunkLengthKey, _chunkLengthDefault));
        set => Preferences.Set(_chunkLengthKey, value.TotalSeconds);
    }

    public TimeSpan OverlapSize => TimeSpan.FromSeconds(2);

    public TimeSpan Epsilon => TimeSpan.FromMilliseconds(300);
}
