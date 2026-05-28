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
        mauiMediaElement.PropertyChanged += OnMediaPlayerPropertyChanged;
        adaptiveLayout.PropertyChanged += OnLayoutPropertyChanged;

        SubscribeToGestures();
        ConfigureLayout();
        //ConfigurePlayerControls();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        var vm = (PlayerWithSubtitlesViewModel)BindingContext;
        mauiMediaElement.Stop();
        mauiMediaElement.Handler?.DisconnectHandler();
        mauiMediaElement.Dispose();
        playerControls.Dispose();
        DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
        vm.SubsScrollRequested -= OnSubScrollRequested;
        vm.TranslationsScrollRequested -= OnTranslationScrollRequested;
        vm.SubtitlesAdapter.Dispose();
        vm.TranslationsAdapter.Dispose();
        mauiMediaElement.PropertyChanged -= OnMediaPlayerPropertyChanged;
        playerGestureRecognizer.PanUpdated -= HandlePanGesture;
        playerControlsGestureRecognizer.PanUpdated -= HandlePanGesture;
        subtitlesGestureRecognizer.PanUpdated -= HandlePanGesture;
        adaptiveLayout.PropertyChanged -= OnLayoutPropertyChanged;
    }

    protected override bool OnBackButtonPressed()
    {
        ScreenStateHelper.RestoreScreen();

        return false;
    }

    private void OnLayoutPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(adaptiveLayout.Height) && isPortraitMode)
        {
            RecalculateVerticalLayout(mauiMediaElement.MediaHeight, mauiMediaElement.MediaWidth);
        }
    }

    private void OnFullScreenToggled(object? sender, StateBtnEventArgs e)
    {
        var isLandscape = e.IsToggled;
        ScreenStateHelper.ChangeOrientation(isLandscape);
        isPortraitMode = !isLandscape;

        if (isPortraitMode)
        {
            adaptiveLayout.Orientation = StackOrientation.Vertical;
        }
        else
        {
            adaptiveLayout.Orientation = StackOrientation.Horizontal;
        }
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

        if (isPortraitMode)
        {
            adaptiveLayout.Orientation = StackOrientation.Vertical;
        }
        else
        {
            adaptiveLayout.Orientation = StackOrientation.Horizontal;
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
            (e.PropertyName == nameof(mauiMediaElement.MediaHeight) || e.PropertyName == nameof(mauiMediaElement.MediaWidth))
            && isPortraitMode
        )
        {
            RecalculateVerticalLayout(mauiMediaElement.MediaHeight, mauiMediaElement.MediaWidth);
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
            AdaptiveLayout.SetRelativeVerticalLength(mauiMediaElement, _layoutSettings.PlayerVerticalLength);
            AdaptiveLayout.SetRelativeVerticalLength(subtitlesView, _layoutSettings.SubtitlesVerticalLength);
        }
    }

    private void ConfigureLayout()
    {
        AdaptiveLayout.SetRelativeHorizontalLength(mauiMediaElement, _layoutSettings.PlayerHorizontalLength);
        AdaptiveLayout.SetRelativeVerticalLength(mauiMediaElement, _layoutSettings.PlayerVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(subtitlesView, _layoutSettings.SubtitlesHoritzontalLength);
        AdaptiveLayout.SetRelativeVerticalLength(subtitlesView, _layoutSettings.SubtitlesVerticalLength);
    }

    private void SubscribeToGestures()
    {
        playerGestureRecognizer.PanUpdated += HandlePanGesture;
        subtitlesGestureRecognizer.PanUpdated += HandlePanGesture;
        playerControlsGestureRecognizer.PanUpdated += HandlePanGesture;
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

                break;
            case GestureStatus.Running:

                if (!(panGestureState.Id == e.GestureId && panGestureState.Locked))
                {
                    return;
                }

                panGestureState.RelativeProgress = Normalize(e.TotalX, e.TotalY);

                InterpolateLayout(panGestureState.RelativeProgress);

                break;
            case GestureStatus.Completed:

                if (!(panGestureState.Id == e.GestureId && panGestureState.Locked))
                {
                    return;
                }

                if (panGestureState.RelativeProgress >= panGestureState.PanThreshold)
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
        AdaptiveLayout.SetRelativeVerticalLength(mauiMediaElement, 1);
        AdaptiveLayout.SetRelativeHorizontalLength(mauiMediaElement, 1);
        AdaptiveLayout.SetRelativeVerticalLength(subtitlesView, _layoutSettings.SubtitlesVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(subtitlesView, _layoutSettings.SubtitlesHoritzontalLength);
        mauiMediaElement.ResetTransformations();
        subtitlesView.ResetTransformations();
    }

    private void RestoreNormalState()
    {
        AdaptiveLayout.SetRelativeVerticalLength(mauiMediaElement, _layoutSettings.PlayerVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(mauiMediaElement, _layoutSettings.PlayerHorizontalLength);
        AdaptiveLayout.SetRelativeVerticalLength(subtitlesView, _layoutSettings.SubtitlesVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(subtitlesView, _layoutSettings.SubtitlesHoritzontalLength);
        mauiMediaElement.ResetTransformations();
        subtitlesView.ResetTransformations();
    }

    private async Task RevertTransition()
    {
        var tasksList = new List<Task>();

        foreach (var child in adaptiveLayout.Children)
        {
            if (child is not VisualElement visualElement)
            {
                throw new InvalidOperationException($"Child is not a VisualElement");
            }

            var translateX = NativeAnimation.AnimateAsync(
                visualElement.TranslationX,
                0,
                (v) => visualElement.TranslationX = v
            );
            var translateY = NativeAnimation.AnimateAsync(
                visualElement.TranslationY,
                0,
                (v) => visualElement.TranslationY = v
            );
            var scale = NativeAnimation.AnimateAsync(visualElement.Scale, 1, (v) => visualElement.Scale = v);

            tasksList.Add(translateX);
            tasksList.Add(translateY);
            tasksList.Add(scale);
        }

        await Task.WhenAll(tasksList);
    }

    private async Task CompleteTransition()
    {
        var oldState = _layoutStateManager.PeekSnapshot(2);
        var newState = _layoutStateManager.PeekSnapshot(1);

        var tasksList = new List<Task>();

        for (int i = 0; i < adaptiveLayout.Children.Count; i++)
        {
            if (adaptiveLayout.Children[i] is not VisualElement visualElement)
            {
                throw new InvalidOperationException($"Child is not a VisualElement");
            }

            var transformation = ViewTransformHelper.CalculateTransformation(
                oldState.ChildrenStates[i].GetBounds(),
                newState.ChildrenStates[i].GetBounds()
            );

            var translateX = NativeAnimation.AnimateAsync(
                visualElement.TranslationX,
                transformation.TranslateX,
                (v) => visualElement.TranslationX = v
            );
            var translateY = NativeAnimation.AnimateAsync(
                visualElement.TranslationY,
                transformation.TranslateY,
                (v) => visualElement.TranslationY = v
            );
            var scale = NativeAnimation.AnimateAsync(
                visualElement.Scale,
                transformation.Scale,
                (v) => visualElement.Scale = v
            );

            tasksList.Add(translateX);
            tasksList.Add(translateY);
            tasksList.Add(scale);
        }

        await Task.WhenAll(tasksList);
    }

    private void InterpolateLayout(double relativeProgress)
    {
        if (relativeProgress < 0 || relativeProgress > 1)
        {
            throw new ArgumentException("Progress must be between 0 and 1");
        }

        var oldState = _layoutStateManager.PeekSnapshot(2);
        var newState = _layoutStateManager.PeekSnapshot(1);

        for (int i = 0; i < adaptiveLayout.Children.Count; i++)
        {
            var oldBounds = oldState.ChildrenStates[i].GetBounds();
            var newBounds = newState.ChildrenStates[i].GetBounds();

            if (adaptiveLayout.Children[i] is not VisualElement child)
            {
                throw new InvalidOperationException($"Child {i} is not a VisualElement");
            }

            var intermediateBounds = Lerp(relativeProgress, oldBounds, newBounds);

            Transformation transformation;

            if (oldBounds == intermediateBounds)
            {
                transformation = new Transformation(1, 0, 0);
            }
            else
            {
                transformation = ViewTransformHelper.CalculateTransformation(oldBounds, intermediateBounds);
            }

            child.Transform(transformation);
        }
    }

    private static Rect Lerp(double progress, Rect oldRect, Rect newRect)
    {
        var x = oldRect.X + (newRect.X - oldRect.X) * progress;
        var y = oldRect.Y + (newRect.Y - oldRect.Y) * progress;
        var width = oldRect.Width + (newRect.Width - oldRect.Width) * progress;
        var height = oldRect.Height + (newRect.Height - oldRect.Height) * progress;

        return new Rect(x, y, width, height);
    }

    private double Normalize(double totalX, double totalY)
    {
        var coefficient = subtitlesHidden ? -1d : 1d;

        var absoluteProgress = isPortraitMode ? totalY : totalX;

        var progress = coefficient * absoluteProgress / subtitlesView.Height;

        return Math.Clamp(progress, 0, 1);
    }

    #endregion
}
