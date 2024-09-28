using Com.Arthenica.Ffmpegkit;
using Java.Interop;
using SubtitlesApp.Maui.Interfaces;
using SubtitlesApp.Maui.Interfaces.Socket;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Maui.Services.Sockets;
using SubtitlesApp.Core.DTOs;
using System.Globalization;
using System.Runtime.CompilerServices;

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
            AudioFormat = AudioFormats.Wave
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

    public partial async Task<TrimmedAudioDto> ExtractAudioAsync(string sourcePath, TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken)
    {
        FFmpegKitConfig.IgnoreSignal(Signal.Sigxcpu);

        _audioMetadata.SetTimeBoundaries(startTime, endTime);

        var ffmpegCommand =
            $"-ss {startTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)} " +
            $"-to {endTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)} " +
            $"-i \"{sourcePath}\" " +
            $"-vn " +
            $"-ar {_audioMetadata.SampleRate} " +
            $"-ac {_audioMetadata.ChannelsCount} " +
            $"-y " +
            $"-f {_audioMetadata.AudioFormat} ";

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

        var audioBytes = await GetAudioBytesAsync(cancellationToken);

        var trimmedAudioMetadata = new TrimmedAudioDto()
        {
            AudioBytes = audioBytes,
            AudioFormat = _audioMetadata.AudioFormat,
            SampleRate = _audioMetadata.SampleRate,
            ChannelsCount = _audioMetadata.ChannelsCount,
            StartTimeOffset = _audioMetadata.StartTimeOffset,
            EndTime = _audioMetadata.EndTime
        };

        return trimmedAudioMetadata;
    }

    private async Task<byte[]> GetAudioBytesAsync(
        CancellationToken cancellationToken)
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

    public FfmpegCallback()
    {
    }

    public void Apply(FFmpegSession? p0)
    {
    }

    public void Disposed()
    {
    }

    public void DisposeUnlessReferenced()
    {
    }

    public void Finalized()
    {
    }

    public void SetJniIdentityHashCode(int value)
    {
    }

    public void SetJniManagedPeerState(JniManagedPeerStates value)
    {
    }

    public void SetPeerReference(JniObjectReference reference)
    {
    }
}
