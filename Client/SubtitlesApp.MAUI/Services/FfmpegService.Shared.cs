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
}
