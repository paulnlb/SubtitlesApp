using SubtitlesApp.Infrastructure.Constants;

namespace SubtitlesApp.Infrastructure.Services.FfmpegNative;

public class FfmpegNativeService
{
    public async Task<Stream> ExtractAudioAsync(
        string sourcePath,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken
    )
    {
        if (!IsRemoteUrl(sourcePath))
        {
            sourcePath = Uri.UnescapeDataString(sourcePath);
        }

        if (string.IsNullOrEmpty(sourcePath))
        {
            throw new ArgumentException("Source path cannot be null or empty.", nameof(sourcePath));
        }

        var outputStream = new MemoryStream();

        await Task.Run(() =>
        {
            var exitCode = FfmpegNativeWrapper.ExtractToStream(
                sourcePath,
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

        outputStream.Position = 0;
        return outputStream;
    }

    public async Task<Stream> ExtractAudioAsync(
        Stream mediaStream,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken
    )
    {
        var outputStream = new MemoryStream();

        await Task.Run(() =>
        {
            var exitCode = FfmpegNativeWrapper.ExtractFromStreamToStream(
                mediaStream,
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

        outputStream.Position = 0;
        return outputStream;
    }

    private static bool IsRemoteUrl(string path)
    {
        var uriCreated = Uri.TryCreate(path, UriKind.Absolute, out var uriResult);
        return uriCreated && (uriResult!.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
