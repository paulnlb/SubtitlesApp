using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SubtitlesApp.Views;
using SubtitlesApp.ViewModels;
using SubtitlesApp.Application.Interfaces;
using SubtitlesApp.Settings;
using SubtitlesApp.Extensions;

#if ANDROID
using SubtitlesApp.Infrastructure.Android.Services.MediaProcessors.Ffmpeg;
#endif

namespace SubtitlesApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddTransientWithShellRoute<MediaElementPage, MediaElementViewModel>("MediaElement");

            builder.Services.AddSingleton(Preferences.Default);

#if RELEASE
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
#else
            builder.Services.AddSingleton<ISettingsService, SettingsServiceDevelopment>();
#endif
            builder.Services.AddSubtitlesAppServices();

            return builder.Build();
        }
    }
}
