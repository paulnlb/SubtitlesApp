using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Infrastructure.Services.FfmpegNative;

public class FfmpegAudioExtractor : IAudioExtractor
{
    private readonly string _sourcePath;

    public FfmpegAudioExtractor(string sourcePath)
    {
        if (!IsRemoteUrl(sourcePath))
        {
            sourcePath = Uri.UnescapeDataString(sourcePath);
        }

        if (string.IsNullOrEmpty(sourcePath))
        {
            throw new ArgumentException("Source path cannot be null or empty.", nameof(sourcePath));
        }

        _sourcePath = sourcePath;
    }

    public async Task<Stream> ExtractAudioAsync(TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken)
    {
        var outputStream = new MemoryStream();

        await Task.Run(() =>
        {
            var exitCode = FfmpegNativeWrapper.ExtractToStream(
                _sourcePath,
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
