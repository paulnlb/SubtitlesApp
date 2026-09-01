using SubtitlesApp.Core.Models;
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

    Task<Result<Stream>> DeleteAudioChunksAsync(
        Stream srcAudio,
        string audioFormat,
        IEnumerable<TimeInterval> keepZones,
        CancellationToken cancellationToken = default
    );
}
