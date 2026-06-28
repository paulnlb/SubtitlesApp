using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class TranscriptionSettings : ITranscriptionSettings
{
    private const string _chunkLengthKey = "chunk_length_key";
    private const double _chunkLengthDefault = 30;

    private const string _subtitltesAsPromptKey = "subtitles_as_prompt_key";
    private const int _subtitltesAsPromptDefault = 0;

    private const string _overlapKey = "overlap_seconds";
    private const double _overlapDefault = 5;

    private const string _epsilonKey = "epsilon_milliseconds";
    private const double _epsilonDefault = 100;

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

    public TimeSpan OverlapSize
    {
        get => TimeSpan.FromSeconds(Preferences.Get(_overlapKey, _overlapDefault));
        set => Preferences.Set(_overlapKey, value.TotalSeconds);
    }

    public TimeSpan Epsilon
    {
        get => TimeSpan.FromMilliseconds(Preferences.Get(_epsilonKey, _epsilonDefault));
        set => Preferences.Set(_epsilonKey, value.TotalMilliseconds);
    }
}
