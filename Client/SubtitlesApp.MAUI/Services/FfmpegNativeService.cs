using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Services;

public partial class FfmpegNativeService : IMediaProcessor
{
    public partial void Dispose();

    public partial Task<byte[]> ExtractAudioAsync(
        string sourcePath,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken
    );
}
