using SubtitlesApp.Infrastructure.Common.Interfaces;

namespace SubtitlesApp.Helpers;

public static partial class MediaProcessorFactory
{
    public static partial IMediaProcessor CreateFfmpeg(string sourcePath)
    {
        throw new NotImplementedException();
    }

    public static partial IMediaProcessor CreateNative(string sourcePath)
    {
        throw new NotImplementedException();
    }
}
