namespace SubtitlesApp.Infrastructure.Interfaces;

public interface IMediaProcessingService
{
    Task<Stream> ExtractAudioAsync(
        string mediaPath,
        TimeSpan startTime,
        TimeSpan endTime,
        int audioTrackIndex = 0,
        CancellationToken cancellationToken = default
    );
}
