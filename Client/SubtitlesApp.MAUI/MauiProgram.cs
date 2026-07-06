using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core.Services;
using MauiPageFullScreen;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using SubtitlesApp.CustomControls;
using SubtitlesApp.Extensions;
using SubtitlesApp.Services;
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

#if ANDROID
        builder.ConfigureLifecycleEvents(static lifecycleBuilder =>
        {
            lifecycleBuilder.AddAndroid(static androidBuilder =>
            {
                androidBuilder.OnCreate(
                    static (activity, _) =>
                    {
                        if (activity is not AndroidX.AppCompat.App.AppCompatActivity componentActivity)
                        {
                            return;
                        }

                        if (
                            componentActivity.GetFragmentManager()
                            is not AndroidX.Fragment.App.FragmentManager fragmentManager
                        )
                        {
                            return;
                        }

                        fragmentManager.RegisterFragmentLifecycleCallbacks(
                            new FragmentLifecycleManager(new PopupDialogFragmentService()),
                            false
                        );
                    }
                );
            });
        });
#endif

        builder.Services.AddSubtitlesAppServices();

        return builder.Build();
    }
}
