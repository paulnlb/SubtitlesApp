using SubtitlesApp.Infrastructure.Android.Services.MediaProcessors.Ffmpeg;
using SubtitlesApp.Infrastructure.Android.Services.MediaProcessors.NativeCodec;
using SubtitlesApp.Core.Interfaces;

namespace SubtitlesApp.Helpers;

public static partial class MediaProcessorFactory
{
    public static partial IMediaProcessor CreateFfmpeg(string sourcePath)
    {
        return new FfmpegAndroid(sourcePath);
    }

    public static partial IMediaProcessor CreateNative(string sourcePath)
    {
        return new NativeCodecAndroid(sourcePath);
    }
}
