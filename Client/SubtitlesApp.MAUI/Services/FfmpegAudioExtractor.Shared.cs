using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Services;

public partial class FfmpegAudioExtractor : IAudioExtractor
{
    private string _sourcePath = string.Empty;

    public partial Task<Stream> ExtractAudioAsync(TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken);

    public void SetAudio(string mediaPath)
    {
        if (string.IsNullOrEmpty(mediaPath))
        {
            throw new ArgumentException("Source path cannot be null or empty.", nameof(mediaPath));
        }

        _sourcePath = mediaPath;
    }
}
