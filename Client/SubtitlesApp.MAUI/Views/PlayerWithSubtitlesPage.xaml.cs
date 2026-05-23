using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
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
    private bool isFullScreen;
    private bool isImmersiveMode;

    private Transformation playerBeforeInterpolation;
    private readonly ConcurrentDictionary<int, List<GestureStatus>> gestureStatuses = [];
    private double totalY = 0;
    private double totalX = 0;
    private const double PanThreshold = 100;

    private readonly ILayoutSettings _layoutSettings;
    private readonly PlayerSubtitlesStateManager _layoutStateManager;

    public PlayerWithSubtitlesPage(PlayerWithSubtitlesViewModel viewModel, ILayoutSettings layoutSettings)
    {
        InitializeComponent();

        BindingContext = viewModel;

        _layoutSettings = layoutSettings;
        _layoutStateManager = new PlayerSubtitlesStateManager(adaptiveLayout);

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;

        viewModel.SubsScrollRequested += OnSubScrollRequested;
        viewModel.TranslationsScrollRequested += OnTranslationScrollRequested;
        mediaPlayer.PropertyChanged += OnMediaPlayerPropertyChanged;
        adaptiveLayout.SizeChanged += OnLayoutSizeChanged;

        SubscribeToGestures();
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
        playerGestureRecognizer.PanUpdated -= HandlePanGesture;
        subtitlesGestureRecognizer.PanUpdated -= HandlePanGesture;
        adaptiveLayout.SizeChanged -= OnLayoutSizeChanged;

        base.OnNavigatedFrom(args);
    }

    protected override bool OnBackButtonPressed()
    {
        FullScreenHelper.RestoreScreen();

        return false;
    }

    private void OnLayoutSizeChanged(object? sender, EventArgs e)
    {
        RecalculateVerticalLayout(mediaPlayer.MediaHeight, mediaPlayer.MediaWidth);
    }

    private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            ImmersiveOff();
        }
        else if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
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
        _layoutSettings.SubtitlesVerticalLength = 1 - newRelativeHeight;

        AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, _layoutSettings.PlayerVerticalLength);
        AdaptiveLayout.SetRelativeVerticalLength(subtitlesView, _layoutSettings.SubtitlesVerticalLength);
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
        FullScreenHelper.FullScreen();
        SafeAreaHelper.DisableSafeAreas(this);
    }

    private void ImmersiveOff()
    {
        FullScreenHelper.RestoreScreen();
        SafeAreaHelper.ResetSafeAreas(this);
        SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
        adaptiveLayout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
    }

    #region handle pan gesture

    private async void HandlePanGesture(object? sender, PanUpdatedEventArgs e)
    {
        Debug.WriteLine($"Yesture type: {e.StatusType}, Id: {e.GestureId}, X: {e.TotalY}, Y: {e.TotalY}");

        var currentGestureStatuses = gestureStatuses.GetOrAdd(e.GestureId, []);

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                currentGestureStatuses.Add(GestureStatus.Started);

                totalY = 0;
                totalX = 0;

                if (BindingContext is PlayerWithSubtitlesViewModel vm)
                {
                    vm.PlayerControlsVisible = false;
                }

                Init();

                playerBeforeInterpolation = new(mediaPlayer.Scale, mediaPlayer.TranslationX, mediaPlayer.TranslationY);

                break;
            case GestureStatus.Running:
                currentGestureStatuses.Add(GestureStatus.Running);

                if (!currentGestureStatuses.Contains(GestureStatus.Started))
                {
                    return;
                }

                totalY = e.TotalY;
                totalX = e.TotalX;

                InterpolateState(totalY, totalX);

                break;
            case GestureStatus.Completed:

                var isValid =
                    currentGestureStatuses.Contains(GestureStatus.Running)
                    && currentGestureStatuses.Contains(GestureStatus.Started);

                gestureStatuses.Remove(e.GestureId, out var _);

                if (!isValid)
                {
                    return;
                }

                var playerAfterInterpolation = new Transformation(
                    mediaPlayer.Scale,
                    mediaPlayer.TranslationX,
                    mediaPlayer.TranslationY
                );

                var distance = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? totalY : totalX;

                if (playerAfterInterpolation != playerBeforeInterpolation && Math.Abs(distance) >= PanThreshold)
                {
                    await CompleteTransition();
                }
                else
                {
                    await RevertTransition();
                }

                break;
        }
    }

    private void Init()
    {
        AdaptiveLayoutState newState;
        _layoutStateManager.PushCurrentState();

        if (isFullScreen)
        {
            RestoreNormalState();
            newState = _layoutStateManager.PreCalcState();
        }
        else
        {
            SetFullscreenState();
            newState = _layoutStateManager.PreCalcState();
        }

        _layoutStateManager.AddSnapshot(newState);

        VirtualResizeToOld();
    }

    private void SetFullscreenState()
    {
        AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, 1);
        AdaptiveLayout.SetRelativeHorizontalLength(mediaPlayer, 1);
        mediaPlayer.ResetTransformations();
        subtitlesView.ResetTransformations();
    }

    private void RestoreNormalState()
    {
        AdaptiveLayout.SetRelativeVerticalLength(mediaPlayer, _layoutSettings.PlayerVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(mediaPlayer, _layoutSettings.PlayerHorizontalLength);
        mediaPlayer.ResetTransformations();
        subtitlesView.ResetTransformations();
    }

    private async Task CompleteTransition()
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

        isFullScreen = !isFullScreen;
    }

    private async Task RevertTransition()
    {
        var oldState = _layoutStateManager.PeekSnapshot(2);
        var newState = _layoutStateManager.PeekSnapshot(1);

        var mediaBounds = MediaPlayer.GetMediaBounds(
            mediaPlayer.MediaWidth,
            mediaPlayer.MediaHeight,
            newState.ChildrenStates[0].GetBounds()
        );

        var playerTransform = ViewTransformHelper.CalculateTransformation(
            mediaBounds,
            oldState.ChildrenStates[0].GetBounds()
        );

        var oldSubtitlesBounds = oldState.ChildrenStates[1].GetBounds();
        var newSubtitlesBounds = newState.ChildrenStates[1].GetBounds();

        var subtitlesTransform = ViewTransformHelper.CalculateTransformation(
            newSubtitlesBounds,
            new Rect(oldSubtitlesBounds.X, oldSubtitlesBounds.Y, newSubtitlesBounds.Width, newSubtitlesBounds.Height)
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

        _layoutStateManager.RestoreFrom(2);
        _layoutStateManager.AddSnapshot(_layoutStateManager.PeekSnapshot(2));
    }

    private void VirtualResizeToOld()
    {
        var oldState = _layoutStateManager.PeekSnapshot(2);
        var newState = _layoutStateManager.PeekSnapshot(1);

        var playerTransform = ViewTransformHelper.CalculateTransformation(
            MediaPlayer.GetMediaBounds(
                mediaPlayer.MediaWidth,
                mediaPlayer.MediaHeight,
                newState.ChildrenStates[0].GetBounds()
            ),
            oldState.ChildrenStates[0].GetBounds()
        );

        var oldSubtitlesBounds = oldState.ChildrenStates[1].GetBounds();
        var newSubtitlesBounds = newState.ChildrenStates[1].GetBounds();

        var subtitlesTransform = ViewTransformHelper.CalculateTransformation(
            newSubtitlesBounds,
            new Rect(oldSubtitlesBounds.X, oldSubtitlesBounds.Y, newSubtitlesBounds.Width, newSubtitlesBounds.Height)
        );

        mediaPlayer.Transform(playerTransform);
        subtitlesView.Transform(subtitlesTransform);
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
            newSubtitlesBounds.Width,
            newSubtitlesBounds.Height
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

        var mediaBounds = MediaPlayer.GetMediaBounds(mediaPlayer.MediaWidth, mediaPlayer.MediaHeight, newPlayerBounds);

        var playerTransform = ViewTransformHelper.CalculateTransformation(mediaBounds, intermediatePlayerBounds);

        var subtitlesTransform = ViewTransformHelper.CalculateTransformation(
            newSubtitlesBounds,
            intermediateSubtitlesBounds
        );

        mediaPlayer.Transform(playerTransform);
        subtitlesView.Transform(subtitlesTransform);
    }

    #endregion
}
