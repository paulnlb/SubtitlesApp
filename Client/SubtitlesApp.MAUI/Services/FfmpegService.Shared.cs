using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Services;

public partial class FfmpegService : IMediaProcessingService
{
    public partial Task<Result<Stream>> ExtractAudioAsync(
        string mediaPath,
        TimeSpan startTime,
        TimeSpan endTime,
        int audioTrackIndex = 0,
        CancellationToken cancellationToken = default
    );

    public partial Task<Result<Stream>> CopySubtitlesAsync(
        string mediaPath,
        string format,
        int subtitleTrackIndex = 0,
        CancellationToken cancellationToken = default
    );

    public partial Task<Result<Stream>> ExtractSubtitlesAsync(
        string mediaPath,
        string outputFormat,
        int subtitleTrackIndex = 0,
        CancellationToken cancellationToken = default
    );
}
