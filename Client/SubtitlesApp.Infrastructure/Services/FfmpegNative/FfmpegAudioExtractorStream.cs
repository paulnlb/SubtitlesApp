using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Infrastructure.Services.FfmpegNative;

public class FfmpegAudioExtractorStream(Stream sourceStream) : IAudioExtractor
{
    public async Task<Stream> ExtractAudioAsync(TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken)
    {
        var outputStream = new MemoryStream();

        await Task.Run(() =>
        {
            var exitCode = FfmpegNativeWrapper.ExtractFromStreamToStream(
                sourceStream,
                outputStream,
                startTime.TotalSeconds,
                endTime.TotalSeconds,
                16000,
                AudioFormats.Wave
            );

            if (exitCode < 0)
            {
                throw new InvalidOperationException($"FFmpeg extraction failed with exit code {exitCode}.");
            }
        });

        sourceStream.Position = 0;
        outputStream.Position = 0;

        return outputStream;
    }
}
