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

    private double playerOriginalWidth = 0;
    private double playerLastWidth = 0;
    private double subtitlesLastTranslateX = 0;
    private double totalX = 0;

    private const double threshold = 100;

    public PlayerWithSubtitlesPage(PlayerWithSubtitlesViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
        playerSubtitlesPage.PropertyChanged += PlayerSubtitlesPage_PropertyChanged;
    }

    private void PlayerSubtitlesPage_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (
            e.PropertyName == nameof(Height)
            && subtitlesCollection.TranslationY != 0
            && playerSubtitlesPage.Height > subtitlesCollection.TranslationY
        )
        {
            subtitlesCollection.TranslationY = playerSubtitlesPage.Height - playerOriginalHeight;
            mediaPlayer.HeightRequest = playerSubtitlesPage.Height;
        }
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        var vm = (PlayerWithSubtitlesViewModel)BindingContext;
        vm.Clean();
        mediaPlayer.Stop();
        mediaPlayer.DisconnectHandler();
        DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;

        playerSubtitlesPage.PropertyChanged -= PlayerSubtitlesPage_PropertyChanged;

        base.OnNavigatedFrom(args);
    }

    protected override bool OnBackButtonPressed()
    {
        if (Controls.IsFullScreen)
        {
            Controls.RestoreScreen();
        }

        return false;
    }

    private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        subtitlesCollection.TranslationX = 0;
        subtitlesCollection.TranslationY = 0;
        mediaPlayer.HeightRequest = -1;
        mediaPlayer.WidthRequest = -1;

        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            Controls.RestoreScreen();
        }
        else if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
            Controls.FullScreen();
        }
    }

    private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        var vm = (PlayerWithSubtitlesViewModel)BindingContext;
        vm.PlayerControlsVisible = false;

        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            HandlePanGestureVertical(sender, e);
        }
        else if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
            HandlePanGestureHorizontal(sender, e);
        }
    }

    private void HandlePanGestureVertical(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                playerLastHeight = mediaPlayer.Height;
                subtitlesLastTranslateY = subtitlesCollection.TranslationY;
                totalY = 0;
                if (playerOriginalHeight == 0)
                {
                    playerOriginalHeight =
                        AdaptiveLayout.GetRelativeVerticalLength(mediaPlayer)!.Value * Shell.Current.CurrentPage.Height;
                }
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
                    ChangeFullScreenStatusVertical();
                }
                else
                {
                    AnimateBounceBackVertical();
                }

                break;
        }
    }

    private void HandlePanGestureHorizontal(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                playerLastWidth = mediaPlayer.Width;
                subtitlesLastTranslateX = subtitlesCollection.TranslationX;
                totalX = 0;
                if (playerOriginalWidth == 0)
                {
                    playerOriginalWidth =
                        AdaptiveLayout.GetRelativeHorizontalLength(mediaPlayer)!.Value * Shell.Current.CurrentPage.Width;
                }
                break;
            case GestureStatus.Running:

                // Handle a special case for Android: e.TotalX is reset to 0 when the position of a sender changed.
                // In that case, e.TotalX becomes deltaX, so we need to maintain an additional "totalX" variable
                if (DeviceInfo.Platform == DevicePlatform.Android && sender == subtitlesCollection)
                {
                    totalX += e.TotalX;
                }
                else
                {
                    totalX = e.TotalX;
                }

                subtitlesCollection.TranslationX = Math.Clamp(
                    subtitlesLastTranslateX + totalX,
                    0,
                    subtitlesCollection.Width
                );
                mediaPlayer.WidthRequest = Math.Clamp(
                    playerLastWidth + totalX,
                    playerOriginalWidth,
                    Shell.Current.CurrentPage.Width
                );
                break;
            case GestureStatus.Completed:

                if (Math.Abs(totalX) >= threshold)
                {
                    ChangeFullScreenStatusHorizontal();
                }
                else
                {
                    AnimateBounceBackHorizontal();
                }

                break;
        }
    }

    private void AnimateBounceBackHorizontal()
    {
        var animation = new Animation(
            v =>
            {
                subtitlesCollection.TranslationX = v;
                mediaPlayer.WidthRequest = playerOriginalWidth + v;
            },
            subtitlesCollection.TranslationX,
            subtitlesLastTranslateX
        );

        animation.Commit(mediaPlayer, "FullScreen", easing: Easing.Linear);
    }

    private void ChangeFullScreenStatusHorizontal()
    {
        if (subtitlesLastTranslateX == 0 && totalX > 0)
        {
            AnimateFullScreenHorizontal();
        }
        else if (subtitlesLastTranslateX != 0 && totalX < 0)
        {
            AnimateExitFullScreenHorizontal();
        }
    }

    private void AnimateExitFullScreenHorizontal()
    {
        var animation = new Animation(
            v =>
            {
                subtitlesCollection.TranslationX = v;
                mediaPlayer.WidthRequest = playerOriginalWidth + v;
            },
            subtitlesCollection.TranslationX,
            0
        );

        animation.Commit(mediaPlayer, "FullScreen", easing: Easing.Linear);
    }

    private void AnimateFullScreenHorizontal()
    {
        var animation = new Animation(
            v =>
            {
                subtitlesCollection.TranslationX = v;
                mediaPlayer.WidthRequest = playerOriginalWidth + v;
            },
            subtitlesCollection.TranslationX,
            subtitlesCollection.Width
        );

        animation.Commit(mediaPlayer, "FullScreen", easing: Easing.Linear, finished: (_, _) => Controls.FullScreen());
    }

    private void ChangeFullScreenStatusVertical()
    {
        if (subtitlesLastTranslateY == 0 && totalY > 0)
        {
            AnimateFullScreenVertical();
        }
        else if (subtitlesLastTranslateY != 0 && totalY < 0)
        {
            AnimateExitFullScreenVertical();
        }
    }

    private void AnimateBounceBackVertical()
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

    private void AnimateExitFullScreenVertical()
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

        animation.Commit(mediaPlayer, "FullScreen", easing: Easing.Linear, finished: (_, _) => Controls.RestoreScreen());
    }

    private void AnimateFullScreenVertical()
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

        animation.Commit(mediaPlayer, "FullScreen", easing: Easing.Linear, finished: (_, _) => Controls.FullScreen());
    }
}
