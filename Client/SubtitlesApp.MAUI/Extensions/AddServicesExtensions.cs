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
        services.AddHttpClient<ISubtitlesService, SubtitlesService>();
    }
}
