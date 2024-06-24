using SubtitlesApp.Application.Interfaces;
using SubtitlesApp.Application.Interfaces.Socket;
using SubtitlesApp.Infrastructure.Common.Services.Sockets;
using SubtitlesApp.Infrastructure.Common.Services.Clients;

#if ANDROID
using SubtitlesApp.Infrastructure.Android.Services.MediaProcessors.Ffmpeg;
#endif

namespace SubtitlesApp.Extensions;

internal static class DependencyInjectionExtensions
{
    public static void AddSubtitlesAppServices(this IServiceCollection services)
    {
        #if ANDROID
        services.AddTransient<IMediaProcessor, FfmpegAndroid>();
        #endif

        services.AddTransient<ISocketListener, UnixSocketListener>();
        services.AddTransient<ISocketSender, UnixSocketSender>();

        services.AddTransient<ISignalRClient, SignalRClient>();
    }
}
