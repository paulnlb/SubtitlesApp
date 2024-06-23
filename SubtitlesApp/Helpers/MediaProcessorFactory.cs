using SubtitlesApp.Application.Interfaces;

namespace SubtitlesApp.Helpers;

public static partial class MediaProcessorFactory
{
    public static partial IMediaProcessor CreateFfmpeg(ISettingsService settings);

    public static partial IMediaProcessor CreateNative(ISettingsService settings);
}
