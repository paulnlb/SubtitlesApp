using SubtitlesApp.Maui.Interfaces;

namespace SubtitlesApp.Services;

public partial class FfmpegService : IMediaProcessor
{
    public partial void Dispose();

    public partial Task<byte[]> ExtractAudioAsync(string sourcePath, TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken);
}
