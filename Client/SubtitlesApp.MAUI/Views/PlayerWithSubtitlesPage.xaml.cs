using System.ComponentModel;
using SubtitlesApp.ClientModels;
using SubtitlesApp.CustomControls;
using SubtitlesApp.Extensions;
using SubtitlesApp.Helpers;
using SubtitlesApp.Interfaces.Settings;
using SubtitlesApp.Layouts;
using SubtitlesApp.ViewModels;
using UraniumUI.Material.Controls;

namespace SubtitlesApp.Views;

public partial class PlayerWithSubtitlesPage : ContentPage
{
    private bool subtitlesHidden;
    private bool isPortraitMode;

    private PanGestureState panGestureState = new();

    private readonly ILayoutSettings _layoutSettings;
    private readonly PlayerSubtitlesStateManager _layoutStateManager;

    public PlayerWithSubtitlesPage(PlayerWithSubtitlesViewModel viewModel, ILayoutSettings layoutSettings)
    {
        InitializeComponent();

        BindingContext = viewModel;

        _layoutSettings = layoutSettings;
        _layoutStateManager = new PlayerSubtitlesStateManager(adaptiveLayout);

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
        isPortraitMode = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait;

        viewModel.SubsScrollRequested += OnSubScrollRequested;
        viewModel.TranslationsScrollRequested += OnTranslationScrollRequested;
        mediaPlayer.PropertyChanged += OnMediaPlayerPropertyChanged;
        adaptiveLayout.PropertyChanged += OnLayoutHeightChanged;

        SubscribeToGestures();
        ConfigureLayout();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        var vm = (PlayerWithSubtitlesViewModel)BindingContext;
        mediaPlayer.Stop();
        mediaPlayer.DisconnectHandler();
        DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
        vm.SubsScrollRequested -= OnSubScrollRequested;
        vm.TranslationsScrollRequested -= OnTranslationScrollRequested;
        vm.SubtitlesAdapter.Dispose();
        vm.TranslationsAdapter.Dispose();
        mediaPlayer.PropertyChanged -= OnMediaPlayerPropertyChanged;
        playerGestureRecognizer.PanUpdated -= HandlePanGesture;
        subtitlesGestureRecognizer.PanUpdated -= HandlePanGesture;
        adaptiveLayout.PropertyChanged -= OnLayoutHeightChanged;
    }

    protected override bool OnBackButtonPressed()
    {
        ScreenStateHelper.RestoreScreen();

        return false;
    }

    private void OnLayoutHeightChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(adaptiveLayout.Height) && isPortraitMode)
        {
            RecalculateVerticalLayout(mediaPlayer.MediaHeight, mediaPlayer.MediaWidth);
        }
    }

    private void OnFullScreenToggled(object? sender, StateBtnEventArgs e)
    {
        var inLandscape = e.IsToggled;
        ScreenStateHelper.ChangeOrientation(inLandscape);
        isPortraitMode = !inLandscape;
    }

    private void OnImmersiveModeToggled(object? sender, StateBtnEventArgs e)
    {
        if (e.IsToggled)
        {
            ImmersiveOn();
        }
        else
        {
            ImmersiveOff();
        }
    }

    private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            isPortraitMode = true;
            ImmersiveOff();
        }
        else if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
            isPortraitMode = false;
            ImmersiveOn();
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
        if (
            (e.PropertyName == nameof(mediaPlayer.MediaHeight) || e.PropertyName == nameof(mediaPlayer.MediaWidth))
            && isPortraitMode
        )
        {
            RecalculateVerticalLayout(mediaPlayer.MediaHeight, mediaPlayer.MediaWidth);
        }
    }

    private void RecalculateVerticalLayout(double videoHeightPx, double videoWidthPx)
    {
        var newRelativeHeight = videoHeightPx * adaptiveLayout.Width / (adaptiveLayout.Height * videoWidthPx);

        if (newRelativeHeight == 0 || double.IsNaN(newRelativeHeight))
        {
            return;
        }

        newRelativeHeight = Math.Min(_layoutSettings.MaxPlayerRelativeVerticalLength, newRelativeHeight);

        _layoutSettings.PlayerVerticalLength = newRelativeHeight;
        _layoutSettings.SubtitlesVerticalLength = 1 - newRelativeHeight;

        if (!subtitlesHidden)
        {
            AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, _layoutSettings.PlayerVerticalLength);
            AdaptiveLayout.SetRelativeVerticalLength(subtitlesView, _layoutSettings.SubtitlesVerticalLength);
        }
    }

    private void ConfigureLayout()
    {
        AdaptiveLayout.SetRelativeHorizontalLength(mediaPlayer, _layoutSettings.PlayerHorizontalLength);
        AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, _layoutSettings.PlayerVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(subtitlesView, _layoutSettings.SubtitlesHoritzontalLength);
        AdaptiveLayout.SetRelativeVerticalLength(subtitlesView, _layoutSettings.SubtitlesVerticalLength);
    }

    private void SubscribeToGestures()
    {
        playerGestureRecognizer.PanUpdated += HandlePanGesture;
        subtitlesGestureRecognizer.PanUpdated += HandlePanGesture;
    }

    private void ImmersiveOn()
    {
        ScreenStateHelper.FullScreen();
        SafeAreaHelper.DisableSafeAreas(this);
    }

    private void ImmersiveOff()
    {
        ScreenStateHelper.RestoreScreen();
        SafeAreaHelper.ResetSafeAreas(this);
        SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
        adaptiveLayout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
    }

    #region handle pan gesture

    private async void HandlePanGesture(object? sender, PanUpdatedEventArgs e)
    {
        if (e.GestureId != panGestureState.Id && panGestureState.Locked)
        {
            return;
        }

        switch (e.StatusType)
        {
            case GestureStatus.Started:

                panGestureState = new() { Id = e.GestureId, Locked = true };

                if (BindingContext is PlayerWithSubtitlesViewModel vm)
                {
                    vm.PlayerControlsVisible = false;
                }

                Init();

                panGestureState.PlayerBeforeInterpolation = mediaPlayer.GetTransformation();

                break;
            case GestureStatus.Running:

                if (!(panGestureState.Id == e.GestureId && panGestureState.Locked))
                {
                    return;
                }

                panGestureState.TotalY = e.TotalY;
                panGestureState.TotalX = e.TotalX;

                InterpolateState(panGestureState.TotalY, panGestureState.TotalX);

                break;
            case GestureStatus.Completed:

                if (!(panGestureState.Id == e.GestureId && panGestureState.Locked))
                {
                    return;
                }

                var playerAfterInterpolation = mediaPlayer.GetTransformation();
                var distance = isPortraitMode ? panGestureState.TotalY : panGestureState.TotalX;

                if (
                    playerAfterInterpolation != panGestureState.PlayerBeforeInterpolation
                    && Math.Abs(distance) >= panGestureState.PanThreshold
                )
                {
                    await CompleteTransition();

                    if (subtitlesHidden)
                    {
                        RestoreNormalState();
                    }
                    else
                    {
                        SetFullscreenState();
                    }

                    subtitlesHidden = !subtitlesHidden;
                }
                else
                {
                    await RevertTransition();
                }

                panGestureState = new();

                break;
        }
    }

    private void Init()
    {
        AdaptiveLayoutState newState;
        _layoutStateManager.PushCurrentState();

        if (subtitlesHidden)
        {
            newState = _layoutStateManager.PreCalcState(
                [_layoutSettings.PlayerVerticalLength, _layoutSettings.SubtitlesVerticalLength],
                [_layoutSettings.PlayerHorizontalLength, _layoutSettings.SubtitlesHoritzontalLength]
            );
        }
        else
        {
            newState = _layoutStateManager.PreCalcState(
                [1, _layoutSettings.SubtitlesVerticalLength],
                [1, _layoutSettings.SubtitlesHoritzontalLength]
            );
        }

        _layoutStateManager.AddSnapshot(newState);
    }

    private void SetFullscreenState()
    {
        AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, 1);
        AdaptiveLayout.SetRelativeHorizontalLength(mediaPlayer, 1);
        AdaptiveLayout.SetRelativeVerticalLength(subtitlesView, _layoutSettings.SubtitlesVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(subtitlesView, _layoutSettings.SubtitlesHoritzontalLength);
        mediaPlayer.ResetTransformations();
        subtitlesView.ResetTransformations();
    }

    private void RestoreNormalState()
    {
        AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, _layoutSettings.PlayerVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(mediaPlayer, _layoutSettings.PlayerHorizontalLength);
        AdaptiveLayout.SetRelativeVerticalLength(subtitlesView, _layoutSettings.SubtitlesVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(subtitlesView, _layoutSettings.SubtitlesHoritzontalLength);
        mediaPlayer.ResetTransformations();
        subtitlesView.ResetTransformations();
    }

    private async Task RevertTransition()
    {
        var playerTranslateX = NativeAnimation.AnimateAsync(
            mediaPlayer.TranslationX,
            0,
            (v) => mediaPlayer.TranslationX = v
        );
        var playerTranslateY = NativeAnimation.AnimateAsync(
            mediaPlayer.TranslationY,
            0,
            (v) => mediaPlayer.TranslationY = v
        );
        var playerScale = NativeAnimation.AnimateAsync(mediaPlayer.Scale, 1, (v) => mediaPlayer.Scale = v);

        var subtitlesTranslateX = NativeAnimation.AnimateAsync(
            subtitlesView.TranslationX,
            0,
            (v) => subtitlesView.TranslationX = v
        );
        var subtitlesTranslateY = NativeAnimation.AnimateAsync(
            subtitlesView.TranslationY,
            0,
            (v) => subtitlesView.TranslationY = v
        );
        var subtitlesScale = NativeAnimation.AnimateAsync(subtitlesView.Scale, 1, (v) => subtitlesView.Scale = v);

        await Task.WhenAll(
            playerTranslateX,
            playerTranslateY,
            playerScale,
            subtitlesTranslateX,
            subtitlesTranslateY,
            subtitlesScale
        );
    }

    private async Task CompleteTransition()
    {
        var oldState = _layoutStateManager.PeekSnapshot(2);
        var newState = _layoutStateManager.PeekSnapshot(1);

        var oldMediaBounds = MediaPlayer.GetMediaBounds(
            mediaPlayer.MediaWidth,
            mediaPlayer.MediaHeight,
            oldState.ChildrenStates[0].GetBounds()
        );

        var playerTransform = ViewTransformHelper.CalculateTransformation(
            oldMediaBounds,
            newState.ChildrenStates[0].GetBounds()
        );

        var oldSubtitlesBounds = oldState.ChildrenStates[1].GetBounds();
        var newSubtitlesBounds = newState.ChildrenStates[1].GetBounds();

        var subtitlesTransform = ViewTransformHelper.CalculateTransformation(
            oldSubtitlesBounds,
            new Rect(newSubtitlesBounds.X, newSubtitlesBounds.Y, oldSubtitlesBounds.Width, oldSubtitlesBounds.Height)
        );

        var playerTranslateX = NativeAnimation.AnimateAsync(
            mediaPlayer.TranslationX,
            playerTransform.TranslateX,
            (v) => mediaPlayer.TranslationX = v
        );
        var playerTranslateY = NativeAnimation.AnimateAsync(
            mediaPlayer.TranslationY,
            playerTransform.TranslateY,
            (v) => mediaPlayer.TranslationY = v
        );
        var playerScale = NativeAnimation.AnimateAsync(
            mediaPlayer.Scale,
            playerTransform.Scale,
            (v) => mediaPlayer.Scale = v
        );

        var subtitlesTranslateX = NativeAnimation.AnimateAsync(
            subtitlesView.TranslationX,
            subtitlesTransform.TranslateX,
            (v) => subtitlesView.TranslationX = v
        );
        var subtitlesTranslateY = NativeAnimation.AnimateAsync(
            subtitlesView.TranslationY,
            subtitlesTransform.TranslateY,
            (v) => subtitlesView.TranslationY = v
        );
        var subtitlesScale = NativeAnimation.AnimateAsync(
            subtitlesView.Scale,
            subtitlesTransform.Scale,
            (v) => subtitlesView.Scale = v
        );

        await Task.WhenAll(
            playerTranslateX,
            playerTranslateY,
            playerScale,
            subtitlesTranslateX,
            subtitlesTranslateY,
            subtitlesScale
        );
    }

    private void InterpolateState(double totalDeltaY, double totalDeltaX)
    {
        var oldState = _layoutStateManager.PeekSnapshot(2);
        var newState = _layoutStateManager.PeekSnapshot(1);

        var oldPlayerBounds = oldState.ChildrenStates[0].GetBounds();
        var newPlayerBounds = newState.ChildrenStates[0].GetBounds();

        var intermediatePlayerBounds = new Rect(
            0,
            0,
            oldPlayerBounds.Width + totalDeltaX,
            oldPlayerBounds.Height + totalDeltaY
        );

        if (oldPlayerBounds.Contains(newPlayerBounds))
        {
            intermediatePlayerBounds.Width = Math.Clamp(
                intermediatePlayerBounds.Width,
                newPlayerBounds.Width,
                oldPlayerBounds.Width
            );
            intermediatePlayerBounds.Height = Math.Clamp(
                intermediatePlayerBounds.Height,
                newPlayerBounds.Height,
                oldPlayerBounds.Height
            );
        }

        if (newPlayerBounds.Contains(oldPlayerBounds))
        {
            intermediatePlayerBounds.Width = Math.Clamp(
                intermediatePlayerBounds.Width,
                oldPlayerBounds.Width,
                newPlayerBounds.Width
            );
            intermediatePlayerBounds.Height = Math.Clamp(
                intermediatePlayerBounds.Height,
                oldPlayerBounds.Height,
                newPlayerBounds.Height
            );
        }

        var oldSubtitlesBounds = oldState.ChildrenStates[1].GetBounds();
        var newSubtitlesBounds = newState.ChildrenStates[1].GetBounds();

        var intermediateSubtitlesBounds = new Rect(
            oldSubtitlesBounds.X + totalDeltaX,
            oldSubtitlesBounds.Y + totalDeltaY,
            oldSubtitlesBounds.Width,
            oldSubtitlesBounds.Height
        );

        intermediateSubtitlesBounds.X = Math.Clamp(
            intermediateSubtitlesBounds.X,
            Math.Min(newSubtitlesBounds.X, oldSubtitlesBounds.X),
            Math.Max(newSubtitlesBounds.X, oldSubtitlesBounds.X)
        );

        intermediateSubtitlesBounds.Y = Math.Clamp(
            intermediateSubtitlesBounds.Y,
            Math.Min(newSubtitlesBounds.Y, oldSubtitlesBounds.Y),
            Math.Max(newSubtitlesBounds.Y, oldSubtitlesBounds.Y)
        );

        Transformation playerTransform;
        Transformation subtitlesTransform;

        if (oldPlayerBounds == intermediatePlayerBounds)
        {
            playerTransform = new Transformation(1, 0, 0);
        }
        else
        {
            var mediaBounds = MediaPlayer.GetMediaBounds(mediaPlayer.MediaWidth, mediaPlayer.MediaHeight, oldPlayerBounds);
            playerTransform = ViewTransformHelper.CalculateTransformation(mediaBounds, intermediatePlayerBounds);
        }

        if (oldSubtitlesBounds == intermediateSubtitlesBounds)
        {
            subtitlesTransform = new Transformation(1, 0, 0);
        }
        else
        {
            subtitlesTransform = ViewTransformHelper.CalculateTransformation(
                oldSubtitlesBounds,
                intermediateSubtitlesBounds
            );
        }

        mediaPlayer.Transform(playerTransform);
        subtitlesView.Transform(subtitlesTransform);
    }

    #endregion
}
