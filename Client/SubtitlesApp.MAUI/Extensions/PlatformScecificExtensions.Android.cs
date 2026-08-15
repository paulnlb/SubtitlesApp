using Android.Util;
using CommunityToolkit.Maui.Core.Services;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using SubtitlesApp.CustomControls;
using SubtitlesApp.Services;

namespace SubtitlesApp.Extensions;

public static partial class PlatformScecificExtensions
{
    public static partial void AddPlatformSpecificBehavior(this MauiAppBuilder builder)
    {
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

        RadioButtonHandler.Mapper.AppendToMapping(
            "CustomRadioButtonSpacing",
            (handler, view) =>
            {
                if (handler.PlatformView is Android.Widget.RadioButton nativeRadioButton)
                {
                    int spacingInDp = 10;
                    int spacingInPx = (int)
                        TypedValue.ApplyDimension(
                            ComplexUnitType.Dip,
                            spacingInDp,
                            nativeRadioButton.Resources?.DisplayMetrics
                        );

                    nativeRadioButton.SetPaddingRelative(spacingInPx, 0, 0, 0);
                }
            }
        );

        ButtonHandler.Mapper.AppendToMapping(
            "CustomButtonAlignment",
            (handler, view) =>
            {
                if (view is not LeftAlignedButton)
                {
                    return;
                }

                handler.PlatformView.Gravity = Android.Views.GravityFlags.Left | Android.Views.GravityFlags.CenterVertical;
            }
        );
    }
}
