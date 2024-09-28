using SubtitlesApp.Maui.Interfaces;
using SubtitlesApp.Core.DTOs;

namespace SubtitlesApp.Services;

public partial class FfmpegService : IMediaProcessor
{
    public partial void Dispose();

    public partial Task<TrimmedAudioDto> ExtractAudioAsync(string sourcePath, TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken);
}
