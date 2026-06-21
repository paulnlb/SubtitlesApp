using SubtitlesApp.Core.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class TranscriptionSettings : ITranscriptionSettings
{
    private const string _chunkLengthKey = "chunk_length_key";
    private const double _chunkLengthDefault = 30;

    private const string _subtitltesAsPromptKey = "subtitles_as_prompt_key";
    private const int _subtitltesAsPromptDefault = 0;

    public TimeSpan ChunkLength
    {
        get => TimeSpan.FromSeconds(Preferences.Get(_chunkLengthKey, _chunkLengthDefault));
        set => Preferences.Set(_chunkLengthKey, value.TotalSeconds);
    }

    public int SubtitlesAsPromptCount
    {
        get => Preferences.Get(_subtitltesAsPromptKey, _subtitltesAsPromptDefault);
        set => Preferences.Set(_subtitltesAsPromptKey, value);
    }

    public TimeSpan OverlapSize => TimeSpan.FromSeconds(5);

    public TimeSpan Epsilon => TimeSpan.FromMilliseconds(100);
}
