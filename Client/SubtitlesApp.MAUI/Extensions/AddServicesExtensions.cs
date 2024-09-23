using SubtitlesApp.Services;
using SubtitlesApp.Maui.Interfaces;
using SubtitlesApp.Maui.Interfaces.Socket;
using SubtitlesApp.Maui.Services.Sockets;
using SubtitlesApp.Maui.Services;

namespace SubtitlesApp.Extensions;

internal static class AddServicesExtensions
{
    public static void AddSubtitlesAppServices(this IServiceCollection services)
    {
        services.AddTransient<IMediaProcessor, FfmpegService>();

        services.AddTransient<IVideoPicker, VideoPicker>();

        services.AddTransient<ISocketListener, UnixSocketListener>();
        services.AddTransient<ISocketSender, UnixSocketSender>();

        services.AddTransient<ISignalRClient, SignalRClient>();
    }
}
