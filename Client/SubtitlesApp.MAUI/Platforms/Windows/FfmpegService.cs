using SubtitlesApp.Maui.Interfaces;
using SubtitlesApp.Core.DTOs;

namespace SubtitlesApp.Services;

public partial class FfmpegService : IMediaProcessor
{
    public partial void Dispose()
    {
        throw new NotImplementedException();
    }

    public partial (TrimmedAudioMetadataDTO Metadata, IAsyncEnumerable<byte[]> AudioBytes) ExtractAudioAsync(string sourcePath, TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
