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
    private double totalX = 0;

    private bool isHorizontalFullScreen;
    private bool isVerticalFullScreen;

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
        SubscribeToGestures();

        _layoutSettings = layoutSettings;

        ConfigureLayout();
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
        playerGestureRecognizer.PanUpdated -= HandlePanGestureHorizontal;
        playerGestureRecognizer.PanUpdated -= HandlePanGestureVertical;
        subtitlesGestureRecognizer.PanUpdated -= HandlePanGestureHorizontal;
        subtitlesGestureRecognizer.PanUpdated -= HandlePanGestureVertical;

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
        SubscribeToGestures();

        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait && !isVerticalFullScreen)
        {
            Controls.RestoreScreen();
        }
        else if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
            Controls.FullScreen();
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

    private void ConfigureLayout()
    {
        _layoutSettings.PlaceholderVerticalLength = PlaceholderHeight / Shell.Current.CurrentPage.Height;
        _layoutSettings.PlaceholderHorizontalLength = 0;

        AdaptiveLayout.SetRelativeVerticalLength(placeholderView, _layoutSettings.PlaceholderVerticalLength);
        AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, _layoutSettings.PlayerVerticalLength);
        AdaptiveLayout.SetRelativeVerticalLength(
            subtitlesCollection,
            _layoutSettings.SubtitlesVerticalLength - _layoutSettings.PlaceholderVerticalLength
        );
        AdaptiveLayout.SetRelativeHorizontalLength(placeholderView, _layoutSettings.PlaceholderHorizontalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(mediaPlayer, _layoutSettings.PlayerHorizontalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(subtitlesCollection, _layoutSettings.SubtitlesHoritzontalLength);
    }

    private void SubscribeToGestures()
    {
        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            playerGestureRecognizer.PanUpdated -= HandlePanGestureHorizontal;
            playerGestureRecognizer.PanUpdated += HandlePanGestureVertical;

            subtitlesGestureRecognizer.PanUpdated -= HandlePanGestureHorizontal;
            subtitlesGestureRecognizer.PanUpdated += HandlePanGestureVertical;
        }
        else if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
            playerGestureRecognizer.PanUpdated -= HandlePanGestureVertical;
            playerGestureRecognizer.PanUpdated += HandlePanGestureHorizontal;

            subtitlesGestureRecognizer.PanUpdated -= HandlePanGestureVertical;
            subtitlesGestureRecognizer.PanUpdated += HandlePanGestureHorizontal;
        }
    }

    #region handle vertical pan gesture

    private void HandlePanGestureVertical(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                totalY = 0;

                if (BindingContext is PlayerWithSubtitlesViewModel vm)
                {
                    vm.PlayerControlsVisible = false;
                }

                break;
            case GestureStatus.Running:
                totalY = e.TotalY;

                VerticalVirtualResize(totalY);

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
        if (!isVerticalFullScreen && totalY > 0)
        {
            AnimateFullScreenVertical();
        }
        else if (isVerticalFullScreen && totalY < 0)
        {
            AnimateExitFullScreenVertical();
        }
    }

    private void AnimateBounceBackVertical()
    {
        NativeAnimation.Animate(subtitlesCollection.TranslationY, 0, VerticalVirtualResize, easing: Easing.Linear);
    }

    private void AnimateExitFullScreenVertical()
    {
        NativeAnimation.Animate(
            subtitlesCollection.TranslationY,
            -subtitlesCollection.Height,
            VerticalVirtualResize,
            easing: Easing.Linear,
            finished: (_, _) =>
            {
                AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, _layoutSettings.PlayerVerticalLength);
                AdaptiveLayout.SetRelativeVerticalLength(placeholderView, _layoutSettings.PlaceholderVerticalLength);
                mediaPlayer.ResetTransformations();
                subtitlesCollection.ResetTransformations();
                placeholderView.ResetTransformations();
                Controls.RestoreScreen();
                isVerticalFullScreen = false;
            }
        );
    }

    private void AnimateFullScreenVertical()
    {
        NativeAnimation.Animate(
            subtitlesCollection.TranslationY,
            subtitlesCollection.Height,
            VerticalVirtualResize,
            easing: Easing.Linear,
            finished: (_, _) =>
            {
                Controls.FullScreen();
                AdaptiveLayout.SetRelativeVerticalLength(placeholderView, 0);
                AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, 1);
                mediaPlayer.ResetTransformations();
                subtitlesCollection.ResetTransformations();
                placeholderView.ResetTransformations();
                isVerticalFullScreen = true;
            }
        );
    }

    private void VerticalVirtualResize(double totalDeltaY)
    {
        if (isVerticalFullScreen)
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

    private void HandlePanGestureHorizontal(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                totalX = 0;

                if (BindingContext is PlayerWithSubtitlesViewModel vm)
                {
                    vm.PlayerControlsVisible = false;
                }

                break;
            case GestureStatus.Running:
                totalX = e.TotalX;

                HorizontalVirtualResize(totalX);
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

    private void ChangeFullScreenStatusHorizontal()
    {
        if (!isHorizontalFullScreen && totalX > 0)
        {
            AnimateFullScreenHorizontal();
        }
        else if (isHorizontalFullScreen && totalX < 0)
        {
            AnimateExitFullScreenHorizontal();
        }
    }

    private void AnimateBounceBackHorizontal()
    {
        NativeAnimation.Animate(subtitlesCollection.TranslationX, 0, HorizontalVirtualResize, easing: Easing.Linear);
    }

    private void AnimateExitFullScreenHorizontal()
    {
        NativeAnimation.Animate(
            subtitlesCollection.TranslationX,
            -subtitlesCollection.Width,
            HorizontalVirtualResize,
            easing: Easing.Linear,
            finished: (_, _) =>
            {
                AdaptiveLayout.SetRelativeHorizontalLength(mediaPlayer, _layoutSettings.PlayerHorizontalLength);
                mediaPlayer.ResetTransformations();
                subtitlesCollection.ResetTransformations();
                isHorizontalFullScreen = false;
            }
        );
    }

    private void AnimateFullScreenHorizontal()
    {
        NativeAnimation.Animate(
            subtitlesCollection.TranslationX,
            subtitlesCollection.Width,
            HorizontalVirtualResize,
            easing: Easing.Linear,
            finished: (_, _) =>
            {
                AdaptiveLayout.SetRelativeHorizontalLength(mediaPlayer, 1);
                mediaPlayer.ResetTransformations();
                subtitlesCollection.ResetTransformations();
                isHorizontalFullScreen = true;
            }
        );
    }

    private void HorizontalVirtualResize(double totalDeltaX)
    {
        if (isHorizontalFullScreen)
        {
            subtitlesCollection.TranslationX = Math.Clamp(totalDeltaX, -subtitlesCollection.Width, 0);
        }
        else
        {
            subtitlesCollection.TranslationX = Math.Clamp(totalDeltaX, 0, subtitlesCollection.Width);
        }

        var transformation = ViewTransformHelper.CalculateTransformation(
            mediaPlayer.GetMediaBounds(),
            new Rect(0, 0, mediaPlayer.Width + subtitlesCollection.TranslationX, adaptiveLayout.Height)
        );

        mediaPlayer.Transform(transformation);
    }
    #endregion
}
