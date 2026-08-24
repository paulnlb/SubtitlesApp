using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Infrastructure.Interfaces;

public interface IMediaProcessingService
{
    Task<Result<Stream>> ExtractAudioAsync(
        string mediaPath,
        TimeSpan startTime,
        TimeSpan endTime,
        int audioTrackIndex = 0,
        CancellationToken cancellationToken = default
    );
}
