using SubtitlesApp.Infrastructure.Android.Services.MediaProcessors.Ffmpeg;
using SubtitlesApp.Infrastructure.Android.Services.MediaProcessors.NativeCodec;
using SubtitlesApp.Application.Interfaces;

namespace SubtitlesApp.Helpers;

public static partial class MediaProcessorFactory
{
    public static partial IMediaProcessor CreateFfmpeg(ISettingsService settings)
    {
        return new FfmpegAndroid(settings);
    }

    public static partial IMediaProcessor CreateNative(ISettingsService settings)
    {
        return new NativeCodecAndroid(settings);
    }
}
