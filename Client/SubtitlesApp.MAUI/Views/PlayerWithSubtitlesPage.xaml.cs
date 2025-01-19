using MauiPageFullScreen;
using SubtitlesApp.Layouts;
using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class PlayerWithSubtitlesPage : ContentPage
{
    private double playerOriginalHeight = 0;
    private double playerLastHeight = 0;
    private double subtitlesLastTranslateY = 0;
    private double totalY = 0;
    private const double threshold = 100;

    public PlayerWithSubtitlesPage(PlayerWithSubtitlesViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        var vm = (PlayerWithSubtitlesViewModel)BindingContext;
        vm.Clean();
        mediaPlayer.Stop();
        mediaPlayer.DisconnectHandler();

        base.OnNavigatedFrom(args);
    }

    protected override bool OnBackButtonPressed()
    {
        var vm = (PlayerWithSubtitlesViewModel)BindingContext;

        // On back button press just show subtitles if they are hidden
        // Otherwise, proceed with exit
        if (!vm.LayoutSettings.IsSideChildVisible)
        {
            vm.LayoutSettings.IsSideChildVisible = true;
            return true;
        }

        if (Controls.IsFullScreen)
        {
            Controls.RestoreScreen();
        }

        return false;
    }

    private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                playerOriginalHeight =
                    AdaptiveLayout.GetRelativeVerticalLength(mediaPlayer)!.Value * Shell.Current.CurrentPage.Height;
                playerLastHeight = mediaPlayer.Height;
                subtitlesLastTranslateY = subtitlesCollection.TranslationY;
                totalY = 0;
                break;
            case GestureStatus.Running:

                // Handle a special case for Android: e.TotalY is reset to 0 when the position of a sender changed.
                // In that case, e.TotalY becomes deltaY, so we need to maintain an additional "totalY" variable
                if (DeviceInfo.Platform == DevicePlatform.Android && sender == subtitlesCollection)
                {
                    totalY += e.TotalY;
                }
                else
                {
                    totalY = e.TotalY;
                }

                subtitlesCollection.TranslationY = Math.Clamp(
                    subtitlesLastTranslateY + totalY,
                    0,
                    subtitlesCollection.Height
                );
                mediaPlayer.HeightRequest = Math.Clamp(
                    playerLastHeight + totalY,
                    playerOriginalHeight,
                    Shell.Current.CurrentPage.Height
                );
                break;
            case GestureStatus.Completed:

                if (Math.Abs(totalY) >= threshold)
                {
                    ChangeFullScreenStatus();
                }
                else
                {
                    AnimateBounceBack();
                }

                break;
        }
    }

    private void ChangeFullScreenStatus()
    {
        if (subtitlesLastTranslateY == 0)
        {
            AnimateFullScreen();
        }
        else
        {
            AnimateExitFullScreen();
        }
    }

    private void AnimateBounceBack()
    {
        var animation = new Animation(
            v =>
            {
                subtitlesCollection.TranslationY = v;
                mediaPlayer.HeightRequest = playerOriginalHeight + v;
            },
            subtitlesCollection.TranslationY,
            subtitlesLastTranslateY
        );

        animation.Commit(mediaPlayer, "FullScreen", easing: Easing.Linear);
    }

    private void AnimateExitFullScreen()
    {
        var animation = new Animation(
            v =>
            {
                subtitlesCollection.TranslationY = v;
                mediaPlayer.HeightRequest = playerOriginalHeight + v;
            },
            subtitlesCollection.TranslationY,
            0
        );

        animation.Commit(mediaPlayer, "FullScreen", easing: Easing.Linear);
    }

    private void AnimateFullScreen()
    {
        var animation = new Animation(
            v =>
            {
                subtitlesCollection.TranslationY = v;
                mediaPlayer.HeightRequest = playerOriginalHeight + v;
            },
            subtitlesCollection.TranslationY,
            subtitlesCollection.Height
        );

        animation.Commit(mediaPlayer, "FullScreen", easing: Easing.Linear);
    }
}
