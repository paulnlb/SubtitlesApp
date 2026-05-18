using System.ComponentModel;
using MauiPageFullScreen;
using SubtitlesApp.Extensions;
using SubtitlesApp.Helpers;
using SubtitlesApp.Interfaces.Settings;
using SubtitlesApp.Layouts;
using SubtitlesApp.ViewModels;
using UraniumUI.Material.Controls;

namespace SubtitlesApp.Views;

public partial class PlayerWithSubtitlesPage : ContentPage
{
    private double totalY = 0;

    private double playerOriginalWidth = 0;
    private double playerLastWidth = 0;
    private double subtitlesLastTranslateX = 0;
    private double totalX = 0;

    private const double PanThreshold = 100;
    private const double PlaceholderHeight = 48;

    private readonly ILayoutSettings _layoutSettings;

    public PlayerWithSubtitlesPage(PlayerWithSubtitlesViewModel viewModel, ILayoutSettings layoutSettings)
    {
        InitializeComponent();

        BindingContext = viewModel;

        SafeAreaHelper.DisableSafeAreas(this);

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;

        viewModel.SubsScrollRequested += OnSubScrollRequested;
        viewModel.TranslationsScrollRequested += OnTranslationScrollRequested;
        mediaPlayer.PropertyChanged += OnMediaPlayerPropertyChanged;

        layoutSettings.PlaceholderVerticalLength = PlaceholderHeight / Shell.Current.CurrentPage.Height;
        layoutSettings.PlaceholderHorizontalLength = 0;

        AdaptiveLayout.SetRelativeVerticalLength(placeholderView, layoutSettings.PlaceholderVerticalLength);
        AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, layoutSettings.PlayerVerticalLength);
        AdaptiveLayout.SetRelativeVerticalLength(
            subtitlesCollection,
            layoutSettings.SubtitlesVerticalLength - layoutSettings.PlaceholderVerticalLength
        );
        AdaptiveLayout.SetRelativeHorizontalLength(placeholderView, layoutSettings.PlaceholderHorizontalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(mediaPlayer, layoutSettings.PlayerHorizontalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(subtitlesCollection, layoutSettings.SubtitlesHoritzontalLength);

        _layoutSettings = layoutSettings;
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
        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            Controls.RestoreScreen();
        }
        else if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
            Controls.FullScreen();
        }
    }

    // TODO: Instead of using this handler, subscribe the HandlePanGestureVertical or HandlePanGestureHorizontal
    // handlers directly depending on the current device orientation
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

        newRelativeHeight = Math.Min(_layoutSettings.MaxPlayerRelativeVerticalLength, newRelativeHeight);

        _layoutSettings.PlayerVerticalLength = newRelativeHeight;
        _layoutSettings.SubtitlesVerticalLength = 1 - newRelativeHeight - _layoutSettings.PlaceholderVerticalLength;

        AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, _layoutSettings.PlayerVerticalLength);
        AdaptiveLayout.SetRelativeVerticalLength(subtitlesCollection, _layoutSettings.SubtitlesVerticalLength);
    }

    #region handle vertical pan gesture

    private void HandlePanGestureVertical(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                totalY = 0;

                break;
            case GestureStatus.Running:
                totalY = e.TotalY;

                TransformLayoutVirtually(totalY);

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
        if (!Controls.IsFullScreen && totalY > 0)
        {
            AnimateFullScreenVertical();
        }
        else if (Controls.IsFullScreen && totalY < 0)
        {
            AnimateExitFullScreenVertical();
        }
    }

    private void AnimateBounceBackVertical()
    {
        var animation = new Animation(TransformLayoutVirtually, subtitlesCollection.TranslationY, 0);

        animation.Commit(mediaPlayer, "BounceBackVertical", easing: Easing.Linear);
    }

    private void AnimateExitFullScreenVertical()
    {
        var animation = new Animation(
            TransformLayoutVirtually,
            subtitlesCollection.TranslationY,
            -subtitlesCollection.Height
        );

        animation.Commit(
            mediaPlayer,
            "ExitFullScreenVertical",
            easing: Easing.Linear,
            finished: (_, _) =>
            {
                AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, _layoutSettings.PlayerVerticalLength);
                AdaptiveLayout.SetRelativeVerticalLength(placeholderView, _layoutSettings.PlaceholderVerticalLength);
                mediaPlayer.ResetTransformations();
                subtitlesCollection.ResetTransformations();
                placeholderView.ResetTransformations();
                Controls.RestoreScreen();
            }
        );
    }

    private void AnimateFullScreenVertical()
    {
        var animation = new Animation(
            TransformLayoutVirtually,
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
                AdaptiveLayout.SetRelativeVerticalLength(placeholderView, 0);
                AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, 1);
                mediaPlayer.ResetTransformations();
                subtitlesCollection.ResetTransformations();
                placeholderView.ResetTransformations();
            }
        );
    }

    private void TransformLayoutVirtually(double totalDeltaY)
    {
        if (Controls.IsFullScreen)
        {
            subtitlesCollection.TranslationY = Math.Clamp(totalDeltaY, -subtitlesCollection.Height, 0);
        }
        else
        {
            subtitlesCollection.TranslationY = Math.Clamp(totalDeltaY, 0, subtitlesCollection.Height);
        }

        var playerTranslateY = -PlaceholderHeight * subtitlesCollection.TranslationY / subtitlesCollection.Height;

        placeholderView.TranslationY = playerTranslateY;

        var transformation = ViewTransformHelper.CalculateTransformation(
            mediaPlayer.GetMediaBounds(),
            new Rect(
                0,
                playerTranslateY,
                adaptiveLayout.Width,
                mediaPlayer.Height + subtitlesCollection.TranslationY - playerTranslateY
            )
        );

        mediaPlayer.Transform(transformation);
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
