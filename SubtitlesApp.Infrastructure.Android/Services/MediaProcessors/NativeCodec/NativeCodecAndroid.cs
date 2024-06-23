using Android.Media;
using Java.Lang;
using SubtitlesApp.Application.Interfaces;
using SubtitlesApp.Application.Interfaces.Socket;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Infrastructure.Common.Services.Sockets;
using SubtitlesApp.Shared.DTOs;

namespace SubtitlesApp.Infrastructure.Android.Services.MediaProcessors.NativeCodec;

public class NativeCodecAndroid : IMediaProcessor
{
    readonly MediaExtractor _mediaExtractor;

    MediaFormat _format;
    string _srcPath;

    readonly TrimmedAudioMetadata _audioMetadata;

    readonly ISocketListener _socketListener;
    ISocketSender _socketSender;
    readonly ISettingsService _settings;

    bool _disposed;

    public NativeCodecAndroid(ISettingsService settings)
    {
        _socketListener = new UnixSocketListener(settings);
        _socketListener.StartListening();

        _settings = settings;

        _mediaExtractor = new MediaExtractor();

        _audioMetadata = new();
    }

    public (TrimmedAudioMetadataDTO Metadata, IAsyncEnumerable<byte[]> AudioBytes) ExtractAudioAsync(string sourcePath, TimeSpan startTime, int duration, CancellationToken cancellationToken)
    {
        _audioMetadata.SetTimeBoundaries(startTime, duration);

        SetDataSource(sourcePath);

        _socketSender = _socketListener switch
        {
            UnixSocketListener => new UnixSocketSender(_settings),
            _ => throw new NotSupportedException($"{_socketListener.GetType()} is not supported"),
        };

        _socketSender.Connect();

        var asyncCodec = new AsyncAndroidCodec(
            _mediaExtractor,
            _format,
            startTime,
            _audioMetadata.EndTime,
            _socketSender);

        asyncCodec.Configure();
        asyncCodec.Start();

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
            _mediaExtractor.Release();
        }

        _disposed = true;
    }

    private async IAsyncEnumerable<byte[]> GetAudioChunks(int chunkSize)
    {
        await foreach (var bytes in _socketListener.ReceiveAsync(chunkSize))
        {
            yield return bytes;
        }

        _socketSender.Close();
    }

    private void SetDataSource(string sourcePath)
    {
        if (sourcePath == _srcPath)
        {
            return;
        }

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
            throw new RuntimeException("No audio track found in the file.");
        }

        _audioMetadata.SampleRate = _format.GetInteger(MediaFormat.KeySampleRate);
        _audioMetadata.ChannelsCount = _format.GetInteger(MediaFormat.KeyChannelCount);
        _audioMetadata.AudioFormat = AudioFormats.PCM;
    }
}
