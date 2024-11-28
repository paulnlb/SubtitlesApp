using IdentityModel.OidcClient;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Interfaces.Socket;
using SubtitlesApp.Services;
using SubtitlesApp.Services.Sockets;

namespace SubtitlesApp.Extensions;

internal static class AddServicesExtensions
{
    public static void AddSubtitlesAppServices(this IServiceCollection services)
    {
        services.AddTransient<IMediaProcessor, FfmpegService>();

        services.AddTransient<IVideoPicker, VideoPicker>();

        services.AddTransient<ISocketListener, UnixSocketListener>();
        services.AddTransient<ISocketSender, UnixSocketSender>();

        services.AddScoped<ISubtitlesService, SubtitlesService>();
        services.AddScoped<IAuthService, AuthService>();

#if DEBUG
        var service = new HttpsClientHandlerService();
        var handler = service.GetPlatformMessageHandler();

        services.AddHttpClient<ISubtitlesService, SubtitlesService>()
            .ConfigurePrimaryHttpMessageHandler(() => handler);
#else
        services.AddHttpClient<ISubtitlesService, SubtitlesService>();
#endif
    }

    public static void AddOidcClient(this IServiceCollection services)
    {
        Func<OidcClientOptions, HttpClient> httpClientFactory = default!;

#if DEBUG
        httpClientFactory = (options) =>
        {
            var handler = new HttpsClientHandlerService();
            return new HttpClient(handler.GetPlatformMessageHandler());
        };
#endif

        // setup OidcClient
        services.AddSingleton(new OidcClient(new()
        {
            Authority = "https://192.168.1.101:7201/identity",
            ClientId = "interactive.public",
            Scope = "openid profile api offline_access",
            HttpClientFactory = httpClientFactory,
            RedirectUri = "subtitlesapp://",
            PostLogoutRedirectUri = "subtitlesapp://",

            Browser = new MauiAuthenticationBrowser()
        }));
    }
}
