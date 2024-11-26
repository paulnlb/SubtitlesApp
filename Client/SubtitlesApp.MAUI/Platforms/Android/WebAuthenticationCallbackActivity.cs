using Android.App;
using Android.Content;
using Android.Content.PM;

namespace SubtitlesApp.Platforms.Android;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter([Intent.ActionView],
              Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
              DataScheme = CALLBACK_SCHEME)]
public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
{
    const string CALLBACK_SCHEME = "subtitlesapp";
}