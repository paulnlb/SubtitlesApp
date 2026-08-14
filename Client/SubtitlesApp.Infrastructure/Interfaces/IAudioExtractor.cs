namespace SubtitlesApp.Infrastructure.Interfaces;

public interface IAudioExtractor
{
    void SetAudio(string mediaPath);

    Task<Stream> ExtractAudioAsync(TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken);
}
