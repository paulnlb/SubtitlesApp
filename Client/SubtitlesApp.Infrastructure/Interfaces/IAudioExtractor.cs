namespace SubtitlesApp.Infrastructure.Interfaces;

public interface IAudioExtractor
{
    Task<Stream> ExtractAudioAsync(TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken);
}
