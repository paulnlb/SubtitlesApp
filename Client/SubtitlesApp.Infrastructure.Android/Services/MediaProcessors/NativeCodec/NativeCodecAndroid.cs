﻿using Android.Media;
using Java.Lang;
using SubtitlesApp.Application.Interfaces;
using SubtitlesApp.Application.Interfaces.Socket;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.DTOs;
using System.Runtime.CompilerServices;

namespace SubtitlesApp.Infrastructure.Android.Services.MediaProcessors.NativeCodec;

public class NativeCodecAndroid : IMediaProcessor
{
    readonly MediaExtractor _mediaExtractor;

    MediaFormat _format;
    string _srcPath;

    readonly TrimmedAudioMetadata _audioMetadata;

    readonly ISocketListener _socketListener;
    readonly ISocketSender _socketSender;

    bool _disposed;

    public NativeCodecAndroid(ISocketListener socketListener, ISocketSender socketSender)
    {
        _socketSender = socketSender;
        _socketListener = socketListener;
        _socketListener.StartListening();

        _mediaExtractor = new MediaExtractor();

        _audioMetadata = new();
    }

    public (TrimmedAudioMetadataDTO Metadata, IAsyncEnumerable<byte[]> AudioBytes) ExtractAudioAsync(
        string sourcePath,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken)
    {
        _audioMetadata.SetTimeBoundaries(startTime, endTime);

        if (sourcePath != _srcPath)
        {
            SetDataSource(sourcePath);
        }

        _socketSender.Connect();

        var asyncCodec = new AsyncAndroidCodec(
            _mediaExtractor,
            _format,
            startTime,
            _audioMetadata.EndTime,
            _socketSender);

        asyncCodec.Configure();
        asyncCodec.Start();

        var bytesEnumerable = GetAudioChunksAsync(
            16 * 1024,
            asyncCodec,
            cancellationToken);

        var trimmedAudioMetadata = new TrimmedAudioMetadataDTO()
        {
            AudioFormat = _audioMetadata.AudioFormat,
            SampleRate = _audioMetadata.SampleRate,
            ChannelsCount = _audioMetadata.ChannelsCount,
            StartTimeOffset = _audioMetadata.StartTimeOffset,
            EndTime = _audioMetadata.EndTime
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

    private async IAsyncEnumerable<byte[]> GetAudioChunksAsync(int chunkSize,
        AsyncAndroidCodec asyncCodec,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        try
        {
            await foreach (var bytes in _socketListener.ReceiveAsync(chunkSize, cancellationToken))
            {
                yield return bytes;
            }
        }
        finally
        {
            asyncCodec.Dispose();

            // allow reuse of the socketSender in the next ExtractAudioAsync invocation
            _socketSender.Disconnect();
        }
    }

    private void SetDataSource(string sourcePath)
    {
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
