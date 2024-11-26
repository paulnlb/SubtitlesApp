namespace SubtitlesApp.Services;

public partial class HttpsClientHandlerService
{
    public partial HttpMessageHandler GetPlatformMessageHandler()
    {
        var handler = new Xamarin.Android.Net.AndroidMessageHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (cert != null && cert.Issuer.Equals("CN=localhost"))
                return true;
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
        return handler;
    }
}

