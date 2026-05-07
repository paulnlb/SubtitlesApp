using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Interfaces.Socket;
using SubtitlesApp.Platforms.Android.FfmpegNative;
using SubtitlesApp.Services.Sockets;

namespace SubtitlesApp.Services;

public partial class FfmpegNativeService : IAudioExtractor
{
    private readonly ISocketListener _socketListener;

    private bool _disposed;

    public FfmpegNativeService(ISocketListener socketListener)
    {
        _socketListener = socketListener;
        _socketListener.StartListening();
    }

    public partial void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _socketListener.Close();
        }

        _disposed = true;
    }

    public async partial Task<byte[]> ExtractAudioAsync(
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

        var outputPath = _socketListener switch
        {
            UnixSocketListener => $"unix://{_socketListener.Endpoint}",
            _ => throw new NotSupportedException($"{_socketListener.GetType()} is not supported"),
        };

        if (string.IsNullOrEmpty(sourcePath))
        {
            return [];
        }

        var cts = new CancellationTokenSource();

        var ffmpegTask = Task.Run(() =>
        {
            var exitCode = FfmpegNativeWrapper.extract_audio(
                (int)startTime.TotalSeconds,
                (int)endTime.TotalSeconds,
                sourcePath,
                16000,
                AudioFormats.Wave,
                $"unix://{_socketListener.Endpoint}"
            );

            if (exitCode < 0)
            {
                cts.Cancel();
            }
        });

        var audioTask = GetAudioBytesAsync(cts.Token);

        await Task.WhenAll(ffmpegTask, audioTask);

        return audioTask.Result;
    }

    private bool IsRemoteUrl(string path)
    {
        var uriCreated = Uri.TryCreate(path, UriKind.Absolute, out var uriResult);
        return uriCreated && (uriResult!.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private async Task<byte[]> GetAudioBytesAsync(CancellationToken cancellationToken)
    {
        var audioChunks = new List<byte>();

        try
        {
            await foreach (var chunk in _socketListener.ReceiveAsync(16 * 1024, cancellationToken))
            {
                audioChunks.AddRange(chunk);
            }
        }
        catch (OperationCanceledException)
        {
            return [];
        }

        return audioChunks.ToArray();
    }
}
