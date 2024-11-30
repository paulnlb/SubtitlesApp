using IdentityModel.OidcClient;
using SubtitlesApp.Core.Services;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Interfaces.Socket;
using SubtitlesApp.Services;
using SubtitlesApp.Services.Sockets;

namespace SubtitlesApp.Extensions;

internal static class ServicesCollectionExtensions
{
    public static void AddSubtitlesAppServices(this IServiceCollection services)
    {
        services.AddTransient<IMediaProcessor, FfmpegService>();

        services.AddTransient<IVideoPicker, VideoPicker>();

        services.AddTransient<ISocketListener, UnixSocketListener>();
        services.AddTransient<ISocketSender, UnixSocketSender>();

        services.AddScoped<ISubtitlesService, SubtitlesService>();
#if DEBUG
        var service = new HttpsClientHandlerService();
        var handler = service.GetPlatformMessageHandler();

        services.AddHttpClient<ISubtitlesService, SubtitlesService>()
            .ConfigurePrimaryHttpMessageHandler(() => handler);
#else
        services.AddHttpClient<ISubtitlesService, SubtitlesService>();
#endif

        services.AddScoped<IdentityModel.OidcClient.Browser.IBrowser, MauiAuthenticationBrowser>();
        services.AddScoped<HttpsClientHandlerService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<LanguageService>();
    }
}
