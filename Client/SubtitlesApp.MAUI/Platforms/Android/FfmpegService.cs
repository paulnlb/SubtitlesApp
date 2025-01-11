using System.Globalization;
using Com.Arthenica.Ffmpegkit;
using Java.Interop;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Interfaces.Socket;
using SubtitlesApp.Services.Sockets;

namespace SubtitlesApp.Services;

public partial class FfmpegService : IMediaProcessor
{
    readonly ISocketListener _socketListener;

    readonly TrimmedAudioMetadata _audioMetadata;

    bool _disposed;

    public FfmpegService(ISocketListener socketListener)
    {
        _socketListener = socketListener;
        _socketListener.StartListening();

        _audioMetadata = new()
        {
            SampleRate = 16000,
            ChannelsCount = 1,
            AudioFormat = AudioFormats.Wave,
        };
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

    public partial Task<byte[]> ExtractAudioAsync(
        string sourcePath,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken
    )
    {
        FFmpegKitConfig.IgnoreSignal(Signal.Sigxcpu);

        _audioMetadata.SetTimeBoundaries(startTime, endTime);

        if (!IsRemoteUrl(sourcePath))
        {
            sourcePath = Uri.UnescapeDataString(sourcePath);
        }

        var ffmpegCommand =
            $"-ss {startTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)} "
            + $"-to {endTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)} "
            + $"-i '{sourcePath}' "
            + "-vn "
            + $"-ar {_audioMetadata.SampleRate} "
            + $"-ac {_audioMetadata.ChannelsCount} "
            + "-y "
            + $"-f {_audioMetadata.AudioFormat} ";

        ffmpegCommand += _socketListener switch
        {
            UnixSocketListener => $"unix://{_socketListener.Endpoint}",
            _ => throw new NotSupportedException($"{_socketListener.GetType()} is not supported"),
        };

        if (cancellationToken.IsCancellationRequested)
        {
            FFmpegKit.Cancel();

            throw new OperationCanceledException("Ffmpeg processing was cancelled");
        }

        if (!string.IsNullOrEmpty(sourcePath))
        {
            var callback = new FfmpegCallback();

            FFmpegKit.ExecuteAsync(ffmpegCommand, callback);
        }

        return GetAudioBytesAsync(cancellationToken);
    }

    private bool IsRemoteUrl(string path)
    {
        var uriCreated = Uri.TryCreate(path, UriKind.Absolute, out var uriResult);
        return uriCreated
            && (uriResult!.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private async Task<byte[]> GetAudioBytesAsync(CancellationToken cancellationToken)
    {
        var audioChunks = new List<byte>();

        await foreach (var chunk in _socketListener.ReceiveAsync(16 * 1024, cancellationToken))
        {
            audioChunks.AddRange(chunk);
        }

        return audioChunks.ToArray();
    }
}

public class FfmpegCallback : Java.Lang.Object, IFFmpegSessionCompleteCallback
{
    public FfmpegCallback() { }

    public void Apply(FFmpegSession? p0) { }

    public void Disposed() { }

    public void DisposeUnlessReferenced() { }

    public void Finalized() { }

    public void SetJniIdentityHashCode(int value) { }

    public void SetJniManagedPeerState(JniManagedPeerStates value) { }

    public void SetPeerReference(JniObjectReference reference) { }
}
