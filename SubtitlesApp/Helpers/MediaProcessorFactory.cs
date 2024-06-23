using SubtitlesApp.Core.Interfaces;

namespace SubtitlesApp.Helpers;

public static partial class MediaProcessorFactory
{
    public static partial IMediaProcessor CreateFfmpeg(string sourcePath);

    public static partial IMediaProcessor CreateNative(string sourcePath);
}
