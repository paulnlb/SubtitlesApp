using CommunityToolkit.Maui;
using MauiPageFullScreen;
using Microsoft.Extensions.Logging;
using SubtitlesApp.Extensions;
using UraniumUI;

namespace SubtitlesApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseFullScreen()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .UseMauiCommunityToolkit(options =>
            {
                options.SetPopupDefaults(
                    new DefaultPopupSettings
                    {
                        BackgroundColor = Colors.Transparent,
                        Padding = 0,
                        Margin = 0,
                    }
                );
                options.SetShouldUseStatusBarBehaviorOnAndroidModalPage(false);
            })
            .UseMauiCommunityToolkitMediaElement(isAndroidForegroundServiceEnabled: true)
            .UseVirtualListView()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.AddPlatformSpecificBehavior();

        builder.Services.AddSubtitlesAppServices();
        builder.Services.AddAppLogging();

        return builder.Build();
    }
}
