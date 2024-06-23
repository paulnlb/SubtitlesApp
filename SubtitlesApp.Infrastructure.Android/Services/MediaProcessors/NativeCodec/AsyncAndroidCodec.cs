using Android.Media;
using SubtitlesApp.Application.Interfaces.Socket;
using System.Diagnostics;
using static Android.Media.MediaCodec;

namespace SubtitlesApp.Infrastructure.Android.Services.MediaProcessors.NativeCodec;

public class AsyncAndroidCodec : IDisposable
{
    readonly MediaExtractor _mediaExtractor;
    readonly MediaCodec _codec;
    readonly MediaFormat _format;
    readonly ISocketSender _socket;

    bool _disposed;

    readonly long _startTimeUs;
    readonly long _endTimeUs;

    readonly TaskCompletionSource _tcs;

    public long StartTimeUs => _startTimeUs;

    public long EndTimeUs => _endTimeUs;

    public MediaExtractor MediaExtractor => _mediaExtractor;

    public AsyncAndroidCodec(
        MediaExtractor mediaExtractor,
        MediaFormat format,
        TimeSpan startTime,
        TimeSpan endTime,
        ISocketSender socket,
        TaskCompletionSource tcs)
    {
        _mediaExtractor = mediaExtractor;
        _format = format;
        _socket = socket;
        _tcs = tcs;

        _startTimeUs = (long)startTime.TotalMicroseconds;
        _endTimeUs = (long)endTime.TotalMicroseconds;

        _mediaExtractor.SeekTo(_startTimeUs, MediaExtractorSeekTo.ClosestSync);

        _codec = CreateDecoderByType(_format.GetString(MediaFormat.KeyMime));
    }

    ~AsyncAndroidCodec() => Dispose();

    public void Configure()
    {
        _codec.SetCallback(new AndroidCodecCallback(this));

        _codec.Configure(_format, null, null, 0);
    }

    public void Start()
    {
        _codec.Start();

        Debug.WriteLine($"{DateTime.Now}: Decoder started");
    }

    public void FinishDecoding()
    {
        Debug.WriteLine($"{DateTime.Now}: Decoder finished");

        _socket.CompleteWrite();

        _tcs.TrySetResult();

        Dispose();
    }

    public void Flush()
    {
        _codec.Flush();
    }

    public void WriteToSocket(byte[] bytes)
    {
        if (_socket == null)
        {
            throw new ArgumentException("Socket wasn't set.");
        }

        _socket.SendAsync(bytes);
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
            _codec.Stop();
            _codec.Release();
        }

        _disposed = true;
    }
}

public class AndroidCodecCallback : Callback
{
    private readonly AsyncAndroidCodec _decoder;

    public AndroidCodecCallback(AsyncAndroidCodec codec)
    {
        _decoder = codec;
    }


    public override void OnError(MediaCodec codec, CodecException e)
    {
    }

    public override void OnInputBufferAvailable(MediaCodec codec, int index)
    {
        using var inputBuffer = codec.GetInputBuffer(index);

        int sampleSize = _decoder.MediaExtractor.ReadSampleData(inputBuffer!, 0);
        long presentationTimeUs = _decoder.MediaExtractor.SampleTime;

        if (sampleSize < 0 || (_decoder.EndTimeUs > 0 && presentationTimeUs > _decoder.EndTimeUs))
        {
            codec.QueueInputBuffer(index, 0, 0, 0, MediaCodecBufferFlags.EndOfStream);
        }
        else
        {
            codec.QueueInputBuffer(index, 0, sampleSize, presentationTimeUs, 0);
            _decoder.MediaExtractor.Advance();
        }
    }

    public override void OnOutputBufferAvailable(MediaCodec codec, int index, BufferInfo info)
    {
        using var outputBuffer = codec.GetOutputBuffer(index);

        byte[] outputData = new byte[info.Size];
        outputBuffer!.Get(outputData);

        _decoder.WriteToSocket(outputData);

        outputBuffer.Clear();

        codec.ReleaseOutputBuffer(index, false);

        if ((info.Flags & MediaCodecBufferFlags.EndOfStream) != 0)
        {
            Debug.WriteLine($"{DateTime.Now}: last buffer processed");
            _decoder.FinishDecoding();
        }
    }

    public override void OnOutputFormatChanged(MediaCodec codec, MediaFormat format)
    {
    }
}
