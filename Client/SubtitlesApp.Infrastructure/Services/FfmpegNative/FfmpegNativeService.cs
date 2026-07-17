using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.Interfaces;

namespace SubtitlesApp.Infrastructure.Services.FfmpegNative;

public class FfmpegNativeService
{
    private readonly ICustomFilePicker _filePicker;

    public FfmpegNativeService(ICustomFilePicker filePicker)
    {
        _filePicker = filePicker;
    }

    public async Task<Stream> ExtractAudioAsync(
        string sourcePath,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken
    )
    {
        IFileResource? fr = null;

        if (Uri.TryCreate(sourcePath, UriKind.Absolute, out Uri? uri) && !IsRemoteUrl(uri))
        {
            var result = _filePicker.GetLocalPath(uri);

            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.Error.Description);
            }

            fr = result.Value;

            sourcePath = fr.Path;
        }

        if (uri is null || !IsRemoteUrl(uri))
        {
            sourcePath = Uri.UnescapeDataString(sourcePath);
        }

        if (string.IsNullOrEmpty(sourcePath))
        {
            throw new ArgumentException("Source path cannot be null or empty.", nameof(sourcePath));
        }

        try
        {
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
        finally
        {
            fr?.Dispose();
        }
    }

    private static bool IsRemoteUrl(Uri uri)
    {
        return uri!.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}
