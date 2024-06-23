using Android.Media;
using Java.Lang;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.Socket;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Infrastructure.Android.Services.MediaProcessors.NativeCodec;

public class NativeCodecAndroid : IMediaProcessor
{
    readonly MediaExtractor _mediaExtractor;
    readonly MediaFormat _format;
    readonly string _srcPath;
    readonly TrimmedAudioMetadata _audioMetadata;

    bool _disposed;

    public NativeCodecAndroid(string sourcePath)
    {
        _mediaExtractor = new MediaExtractor();
        _mediaExtractor.SetDataSource(sourcePath);

        _srcPath = sourcePath;

        int audioTrackIndex = -1;
        for (int i = 0; i < _mediaExtractor.TrackCount; i++)
        {
            _format = _mediaExtractor.GetTrackFormat(i);
            var mime = _format.GetString(MediaFormat.KeyMime);
            if (mime != null && mime.StartsWith("audio/"))
            {
                audioTrackIndex = i;
                _mediaExtractor.SelectTrack(i);
                break;
            }
        }

        if (audioTrackIndex == -1)
        {
            throw new RuntimeException("No audio track found in the video file.");
        }

        _audioMetadata = new()
        {
            SampleRate = _format.GetInteger(MediaFormat.KeySampleRate),
            ChannelsCount = _format.GetInteger(MediaFormat.KeyChannelCount),
            AudioFormat = AudioFormats.PCM,
        };
    }

    public string SourcePath => _srcPath;

    public TrimmedAudioMetadata TrimmedAudioMetadata => _audioMetadata;

    public async Task ExtractAudioAsync(ISocketSender destinationSocket, CancellationToken cancellationToken)
    {
        var startTime = _audioMetadata.StartTimeOffset;
        var endTime = _audioMetadata.EndTime;

        destinationSocket.Connect();

        var tcs = new TaskCompletionSource();

        var asyncCodec = new AsyncAndroidCodec(
            _mediaExtractor,
            _format,
            startTime,
            endTime,
            destinationSocket,
            tcs);

        asyncCodec.Configure();
        asyncCodec.Start();

        using (cancellationToken.Register(() => tcs.TrySetCanceled()))
        {
            await tcs.Task;
        }
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
            _mediaExtractor.Release();
        }

        _disposed = true;
    }
}
