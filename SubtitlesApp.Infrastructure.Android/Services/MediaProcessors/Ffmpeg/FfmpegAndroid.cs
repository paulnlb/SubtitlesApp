using Com.Arthenica.Ffmpegkit;
using Java.Interop;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Infrastructure.Common.Services.Sockets;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Interfaces.Socket;
using SubtitlesApp.Core.Interfaces;

namespace SubtitlesApp.Infrastructure.Android.Services.MediaProcessors.Ffmpeg;

public class FfmpegAndroid : IMediaProcessor
{
    readonly string _srcPath;

    readonly TrimmedAudioMetadata _audioMetadata;

    public FfmpegAndroid(string sourcePath, int sampleRate = 16000, int channelsCount = 1)
    {
        _srcPath = sourcePath;
        _audioMetadata = new()
        {
            SampleRate = sampleRate,
            ChannelsCount = channelsCount,
            AudioFormat = AudioFormats.Wave
        };
    }

    public string SourcePath => _srcPath;

    public TrimmedAudioMetadata TrimmedAudioMetadata => _audioMetadata;

    public void Dispose()
    {
        // Do nothing
    }

    public async Task ExtractAudioAsync(ISocketSender destinationSocket, CancellationToken cancellationToken)
    {
        FFmpegKitConfig.IgnoreSignal(Signal.Sigxcpu);
        var startTime = _audioMetadata.StartTimeOffset;
        var endTime = _audioMetadata.EndTime;

        var ffmpegCommand = $"-y " +
            $"-i \"{_srcPath}\" " +
            $"-ss {startTime.TotalMilliseconds}ms -to {endTime.TotalMilliseconds}ms " +
            $"-vn " +
            $"-ar {_audioMetadata.SampleRate} " +
            $"-ac {_audioMetadata.ChannelsCount} " +
            $"-f {_audioMetadata.AudioFormat} ";

        ffmpegCommand += destinationSocket switch
        {
            UnixSocketSender => $"unix://{destinationSocket.Endpoint}",
            _ => throw new NotSupportedException($"{destinationSocket.GetType()} is not supported"),
        };

        if (cancellationToken.IsCancellationRequested)
        {
            FFmpegKit.Cancel();
        }

        if (!string.IsNullOrEmpty(_srcPath))
        {
            var tcs = new TaskCompletionSource();
            var callback = new FfmpegCallback(tcs);

            FFmpegKit.ExecuteAsync(ffmpegCommand, callback);

            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                await tcs.Task;
            }
        }
    }
}

public class FfmpegCallback : Java.Lang.Object, IFFmpegSessionCompleteCallback
{
    private readonly TaskCompletionSource _tcs;

    public FfmpegCallback(TaskCompletionSource tcs)
    {
        _tcs = tcs;
    }

    public void Apply(FFmpegSession? p0)
    {
        _tcs.TrySetResult();
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
