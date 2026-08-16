using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Services;

public partial class FfmpegService : IMediaProcessingService
{
    public partial Task<Stream> ExtractAudioAsync(
        string mediaPath,
        TimeSpan startTime,
        TimeSpan endTime,
        int audioTrackIndex = 0,
        CancellationToken cancellationToken = default
    );

    public partial Task<Stream> CopySubtitlesAsync(
        string mediaPath,
        string format,
        int subtitleTrackIndex = 0,
        CancellationToken cancellationToken = default
    );

    public partial Task<Stream> ExtractSubtitlesAsync(
        string mediaPath,
        string outputFormat,
        int subtitleTrackIndex = 0,
        CancellationToken cancellationToken = default
    );
}
