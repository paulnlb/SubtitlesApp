using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.Fragment.App;
using CommunityToolkit.Maui.Core;

namespace SubtitlesApp.Services;

sealed class PopupDialogFragmentService : IDialogFragmentService
{
    readonly Dictionary<Fragment, Android.Views.View> trackedViews = new();
    readonly Dictionary<Android.Views.View, OriginalPadding> originalPaddings = new();

    public void OnFragmentStarted(FragmentManager fm, Fragment f)
    {
        if (f is not DialogFragment dialogFragment)
        {
            return;
        }

        if (dialogFragment.Dialog?.Window is not Android.Views.Window window)
        {
            return;
        }

        window.SetSoftInputMode(SoftInput.StateUnspecified | SoftInput.AdjustResize);
    }

    public void OnFragmentViewCreated(FragmentManager fm, Fragment f, Android.Views.View v, Bundle? savedInstanceState)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            return;
        }

        if (f is not DialogFragment)
        {
            return;
        }

        originalPaddings[v] = new OriginalPadding(v.PaddingLeft, v.PaddingTop, v.PaddingRight, v.PaddingBottom);

        trackedViews[f] = v;

        ViewCompat.SetOnApplyWindowInsetsListener(v, new InsetsListener(originalPaddings));

        ViewCompat.RequestApplyInsets(v);
    }

    public void OnFragmentViewDestroyed(FragmentManager fm, Fragment f)
    {
        if (!trackedViews.TryGetValue(f, out var view))
        {
            return;
        }

        ViewCompat.SetOnApplyWindowInsetsListener(view, null);

        trackedViews.Remove(f);
        originalPaddings.Remove(view);
    }

    sealed class InsetsListener(Dictionary<Android.Views.View, OriginalPadding> originalPaddings)
        : Java.Lang.Object,
            IOnApplyWindowInsetsListener
    {
        public WindowInsetsCompat OnApplyWindowInsets(Android.Views.View view, WindowInsetsCompat insets)
        {
            if (!originalPaddings.TryGetValue(view, out var original))
            {
                return insets;
            }

            var imeInsets = insets.GetInsets(WindowInsetsCompat.Type.Ime());

            view.SetPadding(original.Left, original.Top, original.Right, original.Bottom + imeInsets.Bottom);

            return insets;
        }
    }

    readonly record struct OriginalPadding(int Left, int Top, int Right, int Bottom);

    public void OnFragmentAttached(FragmentManager fm, Fragment f, Context context) { }

    public void OnFragmentCreated(FragmentManager fm, Fragment f, Bundle? savedInstanceState) { }

    public void OnFragmentDestroyed(FragmentManager fm, Fragment f) { }

    public void OnFragmentDetached(FragmentManager fm, Fragment f) { }

    public void OnFragmentPaused(FragmentManager fm, Fragment f) { }

    public void OnFragmentPreAttached(FragmentManager fm, Fragment f, Context context) { }

    public void OnFragmentPreCreated(FragmentManager fm, Fragment f, Bundle? savedInstanceState) { }

    public void OnFragmentResumed(FragmentManager fm, Fragment f) { }

    public void OnFragmentSaveInstanceState(FragmentManager fm, Fragment f, Bundle outState) { }

    public void OnFragmentStopped(FragmentManager fm, Fragment f) { }
}
