using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SubtitlesApp.Views;
using SubtitlesApp.ViewModels;
using SubtitlesApp.Interfaces;
using SubtitlesApp.Settings;
using SubtitlesApp.Extensions;
using MauiPageFullScreen;


namespace SubtitlesApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseFullScreen()
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
            builder.Services.AddTransientWithShellRoute<MainPage, MainPageViewModel>("MainPage");
            builder.Services.AddTransientWithShellRoute<SettingsPage, SettingsViewModel>("settings");

            builder.Services.AddSingleton(Preferences.Default);

#if RELEASE
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
#else
            builder.Services.AddSingleton<ISettingsService, SettingsServiceDevelopment>();
#endif
            builder.Services.AddSubtitlesAppServices();
            builder.Services.AddOidcClient();

            return builder.Build();
        }
    }
}
