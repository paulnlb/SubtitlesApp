using Com.Arthenica.Ffmpegkit;
using Java.Interop;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Infrastructure.Common.Services.Sockets;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Application.Interfaces.Socket;
using SubtitlesApp.Application.Interfaces;
using SubtitlesApp.Shared.DTOs;
using Android.Media;

namespace SubtitlesApp.Infrastructure.Android.Services.MediaProcessors.Ffmpeg;

public class FfmpegAndroid : IMediaProcessor
{
    readonly ISocketListener _socketListener;

    readonly TrimmedAudioMetadata _audioMetadata;

    bool _disposed;

    public FfmpegAndroid(ISettingsService settings)
    {
        _audioMetadata = new()
        {
            SampleRate = 16000,
            ChannelsCount = 1,
            AudioFormat = AudioFormats.Wave
        };

        _socketListener = new UnixSocketListener(settings);

        _socketListener.StartListening();
    }

    public void Dispose()
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

    public (TrimmedAudioMetadataDTO Metadata, IAsyncEnumerable<byte[]> AudioBytes) ExtractAudioAsync(string sourcePath, TimeSpan startTime, int duration, CancellationToken cancellationToken)
    {
        FFmpegKitConfig.IgnoreSignal(Signal.Sigxcpu);

        _audioMetadata.SetTimeBoundaries(startTime, duration);

        var ffmpegCommand = $"-y " +
            $"-i \"{sourcePath}\" " +
            $"-ss {startTime.TotalMilliseconds}ms -to {_audioMetadata.EndTime.TotalMilliseconds}ms " +
            $"-vn " +
            $"-ar {_audioMetadata.SampleRate} " +
            $"-ac {_audioMetadata.ChannelsCount} " +
            $"-f {_audioMetadata.AudioFormat} ";

        ffmpegCommand += _socketListener switch
        {
            UnixSocketListener => $"unix://{_socketListener.Endpoint}",
            _ => throw new NotSupportedException($"{_socketListener.GetType()} is not supported"),
        };

        if (cancellationToken.IsCancellationRequested)
        {
            FFmpegKit.Cancel();
        }

        if (!string.IsNullOrEmpty(sourcePath))
        {
            var callback = new FfmpegCallback();

            FFmpegKit.ExecuteAsync(ffmpegCommand, callback);
        }

        var bytesEnumerable = GetAudioChunks(16 * 1024);

        var trimmedAudioMetadata = new TrimmedAudioMetadataDTO()
        {
            AudioFormat = _audioMetadata.AudioFormat,
            SampleRate = _audioMetadata.SampleRate,
            ChannelsCount = _audioMetadata.ChannelsCount,
            StartTimeOffset = _audioMetadata.StartTimeOffset
        };

        return (trimmedAudioMetadata, bytesEnumerable);
    }

    private async IAsyncEnumerable<byte[]> GetAudioChunks(int chunkSize)
    {
        await foreach (var bytes in _socketListener.ReceiveAsync(chunkSize))
        {
            yield return bytes;
        }
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
