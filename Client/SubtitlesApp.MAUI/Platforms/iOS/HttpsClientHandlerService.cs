namespace SubtitlesApp.Services;

public partial class HttpsClientHandlerService
{
    public partial HttpMessageHandler GetPlatformMessageHandler()
    {
        var handler = new NSUrlSessionHandler
        {
            TrustOverrideForUrl = IsHttpsLocalhost
        };
        return handler;
    }

    public bool IsHttpsLocalhost(NSUrlSessionHandler sender, string url, Security.SecTrust trust)
    {
        if (url.StartsWith("https://localhost"))
            return true;
        return false;
    }
}

