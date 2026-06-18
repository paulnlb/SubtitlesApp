using SubtitlesApp.Core.Interfaces;

namespace SubtitlesApp.Services;

public partial class FfmpegNativeService : IAudioExtractor
{
    public partial Task<Stream> ExtractAudioAsync(
        string sourcePath,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken
    );
}
