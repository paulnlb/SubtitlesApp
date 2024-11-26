namespace SubtitlesApp.Services;

public partial class HttpsClientHandlerService
{
    public partial HttpMessageHandler GetPlatformMessageHandler() => throw new PlatformNotSupportedException("Only Android and iOS supported.");
}

