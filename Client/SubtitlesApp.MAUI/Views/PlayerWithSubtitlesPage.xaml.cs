using System.ComponentModel;
using MauiPageFullScreen;
using SubtitlesApp.Helpers;
using SubtitlesApp.Layouts;
using SubtitlesApp.ViewModels;
using UraniumUI.Material.Controls;

namespace SubtitlesApp.Views;

public partial class PlayerWithSubtitlesPage : ContentPage
{
    private double playerOriginalHeight = 0;
    private double subtitlesLastTranslateY = 0;
    private double totalY = 0;

    private double playerOriginalWidth = 0;
    private double playerLastWidth = 0;
    private double subtitlesLastTranslateX = 0;
    private double totalX = 0;

    private const double PanThreshold = 100;
    private const double MaxPlayerRelativeVerticalLength = 0.5;

    public PlayerWithSubtitlesPage(PlayerWithSubtitlesViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        SafeAreaHelper.DisableSafeAreas(this);

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;

        viewModel.SubsScrollRequested += OnSubScrollRequested;
        viewModel.TranslationsScrollRequested += OnTranslationScrollRequested;
        mediaPlayer.PropertyChanged += OnMediaPlayerPropertyChanged;

        AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, 0.3);
        AdaptiveLayout.SetRelativeVerticalLength(subtitlesCollection, 0.7);
        AdaptiveLayout.SetRelativeHorizontalLength(mediaPlayer, 0.65);
        AdaptiveLayout.SetRelativeHorizontalLength(subtitlesCollection, 0.35);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        var vm = (PlayerWithSubtitlesViewModel)BindingContext;
        vm.Clean();
        mediaPlayer.Stop();
        mediaPlayer.DisconnectHandler();
        DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
        subtitlesList.Clean();
        translationsList.Clean();
        vm.SubsScrollRequested += OnSubScrollRequested;
        vm.TranslationsScrollRequested += OnTranslationScrollRequested;
        mediaPlayer.PropertyChanged -= OnMediaPlayerPropertyChanged;

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
        subtitlesCollection.TranslationX = subtitlesCollection.TranslationY = 0;
        mediaPlayer.HeightRequest = mediaPlayer.WidthRequest = -1;

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
        if (e.StatusType == GestureStatus.Started && BindingContext is PlayerWithSubtitlesViewModel vm)
        {
            vm.PlayerControlsVisible = false;
        }

        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            HandlePanGestureVertical(sender, e);
        }
        else if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
            HandlePanGestureHorizontal(sender, e);
        }
    }

    private void OnSelectedTabChanged(object? sender, TabItem e)
    {
        if (BindingContext is not PlayerWithSubtitlesViewModel vm)
        {
            return;
        }

        if (e is null)
        {
            return;
        }
        else if (e.Title == "Subtitles")
        {
            vm.IsSubtitlesSelected = true;
            vm.IsTranslationsSelected = false;
        }
        else if (e.Title == "Translations")
        {
            vm.IsSubtitlesSelected = false;
            vm.IsTranslationsSelected = true;
        }
    }

    private void OnSubScrollRequested(object? sender, EventArgs e)
    {
        if (BindingContext is not PlayerWithSubtitlesViewModel vm)
        {
            return;
        }

        subtitlesList.ScrollToIndex(vm.SubtitlesCollectionState.CurrentSubtitleIndex);
    }

    private void OnTranslationScrollRequested(object? sender, EventArgs e)
    {
        if (BindingContext is not PlayerWithSubtitlesViewModel vm)
        {
            return;
        }

        translationsList.ScrollToIndex(vm.TranslationsCollectionState.CurrentSubtitleIndex);
    }

    private void OnMediaPlayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(mediaPlayer.MediaHeight) || e.PropertyName == nameof(mediaPlayer.MediaWidth))
        {
            RecalculateVerticalLayout(mediaPlayer.MediaHeight, mediaPlayer.MediaWidth);
        }
    }

    private void RecalculateVerticalLayout(double videoHeightPx, double videoWidthPx)
    {
        if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
            return;
        }

        var newRelativeHeight = videoHeightPx * adaptiveLayout.Width / (adaptiveLayout.Height * videoWidthPx);

        if (newRelativeHeight == 0 || double.IsNaN(newRelativeHeight))
        {
            return;
        }

        newRelativeHeight = Math.Min(MaxPlayerRelativeVerticalLength, newRelativeHeight);

        AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, newRelativeHeight);
        AdaptiveLayout.SetRelativeVerticalLength(subtitlesCollection, 1 - newRelativeHeight);
    }

    #region handle vertical pan gesture

    private void HandlePanGestureVertical(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                subtitlesLastTranslateY = subtitlesCollection.TranslationY;
                totalY = 0;
                if (playerOriginalHeight == 0)
                {
                    playerOriginalHeight = mediaPlayer.Height;
                }
                break;
            case GestureStatus.Running:
                totalY = e.TotalY;

                subtitlesCollection.TranslationY = Math.Clamp(
                    subtitlesLastTranslateY + totalY,
                    0,
                    subtitlesCollection.Height
                );
                var bounds = mediaPlayer.GetMediaBounds();
                var transformation = ViewTransformHelper.CalculateTransformation(
                    bounds,
                    new Rect(0, 0, adaptiveLayout.Width, playerOriginalHeight + subtitlesCollection.TranslationY)
                );
                mediaPlayer.Transform(transformation);

                break;
            case GestureStatus.Completed:

                if (Math.Abs(totalY) >= PanThreshold)
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

                var transformation = ViewTransformHelper.CalculateTransformation(
                    mediaPlayer.GetMediaBounds(),
                    new Rect(0, 0, adaptiveLayout.Width, playerOriginalHeight + v)
                );
                mediaPlayer.Transform(transformation);
            },
            subtitlesCollection.TranslationY,
            subtitlesLastTranslateY
        );

        animation.Commit(mediaPlayer, "BounceBackVertical", easing: Easing.Linear);
    }

    private void AnimateExitFullScreenVertical()
    {
        var animation = new Animation(
            v =>
            {
                subtitlesCollection.TranslationY = v;
                var transformation = ViewTransformHelper.CalculateTransformation(
                    mediaPlayer.GetMediaBounds(),
                    new Rect(0, 0, adaptiveLayout.Width, playerOriginalHeight + v)
                );
                mediaPlayer.Transform(transformation);
            },
            subtitlesCollection.TranslationY,
            0
        );

        animation.Commit(
            mediaPlayer,
            "ExitFullScreenVertical",
            easing: Easing.Linear,
            finished: (_, _) =>
            {
                mediaPlayer.HeightRequest = playerOriginalHeight;
                mediaPlayer.ResetTransformations();
                Controls.RestoreScreen();
            }
        );
    }

    private void AnimateFullScreenVertical()
    {
        var animation = new Animation(
            v =>
            {
                subtitlesCollection.TranslationY = v;

                var transformation = ViewTransformHelper.CalculateTransformation(
                    mediaPlayer.GetMediaBounds(),
                    new Rect(0, 0, adaptiveLayout.Width, playerOriginalHeight + v)
                );
                mediaPlayer.Transform(transformation);
            },
            subtitlesCollection.TranslationY,
            subtitlesCollection.Height
        );

        animation.Commit(
            mediaPlayer,
            "FullScreenVertical",
            easing: Easing.Linear,
            finished: (_, _) =>
            {
                Controls.FullScreen();
                mediaPlayer.HeightRequest = subtitlesCollection.Height + playerOriginalHeight;
                mediaPlayer.ResetTransformations();
            }
        );
    }

    #endregion

    #region handle horizontal pan gesture

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

                totalX = e.TotalX;

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

                if (Math.Abs(totalX) >= PanThreshold)
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
    #endregion
}
