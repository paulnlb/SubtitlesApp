using System.ComponentModel;
using CommunityToolkit.Maui.Views;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.EventArgs;
using SubtitlesApp.Extensions;
using SubtitlesApp.Helpers;
using SubtitlesApp.Interfaces.Settings;
using SubtitlesApp.Layouts;
using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class PlayerWithSubtitlesPage : ContentPage
{
    private bool subtitlesHidden;
    private DisplayOrientation currentOrientation = DeviceDisplay.MainDisplayInfo.Orientation;

    private bool IsVerticalLayout
    {
        get => adaptiveLayout.Orientation == StackOrientation.Vertical;
        set
        {
            if (value)
            {
                adaptiveLayout.Orientation = StackOrientation.Vertical;
            }
            else
            {
                adaptiveLayout.Orientation = StackOrientation.Horizontal;
            }
        }
    }

    private PanGestureState panGestureState = new();

    private readonly ILayoutSettings _layoutSettings;
    private readonly AdaptiveLayoutStateManager _layoutStateManager;

    private PlayerWithSubtitlesViewModel Vm => (PlayerWithSubtitlesViewModel)BindingContext;

    public PlayerWithSubtitlesPage(PlayerWithSubtitlesViewModel vm, ILayoutSettings layoutSettings)
    {
        InitializeComponent();

        BindingContext = vm;

        _layoutSettings = layoutSettings;
        _layoutStateManager = new AdaptiveLayoutStateManager(adaptiveLayout);

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;

        vm.CaptionsVm.SeekRequested += OnSeekRequested;
        vm.PropertyChanged += OnVmPropertyChanged;
        mauiMediaElement.PropertyChanged += OnMediaPlayerPropertyChanged;
        adaptiveLayout.PropertyChanged += OnLayoutPropertyChanged;

        mauiMediaElement.SetBinding(
            MediaElement.DurationProperty,
            new Binding(nameof(vm.CaptionsVm.MediaDuration), BindingMode.OneWayToSource, source: vm.CaptionsVm)
        );

        SubscribeToGestures();
        ConfigureLayout();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        mauiMediaElement.Stop();
        mauiMediaElement.Handler?.DisconnectHandler();
        mauiMediaElement.Dispose();
        playerControls.Dispose();
        DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
        Vm.CaptionsVm.SeekRequested -= OnSeekRequested;
        Vm.PropertyChanged -= OnVmPropertyChanged;
        mauiMediaElement.PropertyChanged -= OnMediaPlayerPropertyChanged;
        playerGestureRecognizer.PanUpdated -= HandlePanGesture;
        playerControlsGestureRecognizer.PanUpdated -= HandlePanGesture;
        captionsGestureRecognizer.PanUpdated -= HandlePanGesture;
        adaptiveLayout.PropertyChanged -= OnLayoutPropertyChanged;
        captionsView.Cleanup();
    }

    protected override bool OnBackButtonPressed()
    {
        ScreenStateHelper.RestoreScreen();

        return false;
    }

    private void OnLayoutPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(adaptiveLayout.Height) && IsVerticalLayout)
        {
            RecalculateVerticalLayout(mauiMediaElement.MediaHeight, mauiMediaElement.MediaWidth);
        }
    }

    private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        currentOrientation = DeviceDisplay.MainDisplayInfo.Orientation;
        IsVerticalLayout = currentOrientation == DisplayOrientation.Portrait;
    }

    private void OnMediaPlayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (
            (e.PropertyName == nameof(mauiMediaElement.MediaHeight) || e.PropertyName == nameof(mauiMediaElement.MediaWidth))
            && IsVerticalLayout
        )
        {
            RecalculateVerticalLayout(mauiMediaElement.MediaHeight, mauiMediaElement.MediaWidth);
        }
    }

    private void OnPositionChanged(object? sender, EventArgs e)
    {
        if (
            Vm.CaptionsVm.PositionChangedCommand != null
            && Vm.CaptionsVm.PositionChangedCommand.CanExecute(mauiMediaElement.Position)
        )
            Vm.CaptionsVm.PositionChangedCommand.Execute(mauiMediaElement.Position);
    }

    private void OnSeekRequested(object? sender, SeekEventArgs e)
    {
        mauiMediaElement.SeekTo(e.Time);
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Vm.IsImmersiveOn))
        {
            if (Vm.IsImmersiveOn)
            {
                ImmersiveOn();
            }
            else
            {
                ImmersiveOff();
            }
        }
        else if (e.PropertyName == nameof(Vm.IsFullScreenOn))
        {
            ScreenStateHelper.ChangeOrientation(Vm.IsFullScreenOn);
            Vm.IsImmersiveOn = Vm.IsFullScreenOn;

            if (Vm.IsFullScreenOn)
            {
                IsVerticalLayout = false;
            }
            else
            {
                IsVerticalLayout = currentOrientation == DisplayOrientation.Portrait;
            }
        }
    }

    #region helper methods

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
            AdaptiveLayout.SetRelativeVerticalLength(captionsView, _layoutSettings.SubtitlesVerticalLength);
        }
    }

    private void ConfigureLayout()
    {
        AdaptiveLayout.SetRelativeHorizontalLength(mauiMediaElement, _layoutSettings.PlayerHorizontalLength);
        AdaptiveLayout.SetRelativeVerticalLength(mauiMediaElement, _layoutSettings.PlayerVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(captionsView, _layoutSettings.SubtitlesHoritzontalLength);
        AdaptiveLayout.SetRelativeVerticalLength(captionsView, _layoutSettings.SubtitlesVerticalLength);
    }

    private void SubscribeToGestures()
    {
        playerGestureRecognizer.PanUpdated += HandlePanGesture;
        captionsGestureRecognizer.PanUpdated += HandlePanGesture;
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
        absoluteLayout.SafeAreaEdges =
            adaptiveLayout.SafeAreaEdges =
            this.SafeAreaEdges =
                new SafeAreaEdges(SafeAreaRegions.Container);
    }

    #endregion

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

                RefreshLayoutStates();

                break;
            case GestureStatus.Running:

                if (!(panGestureState.Id == e.GestureId && panGestureState.Locked))
                {
                    return;
                }

                panGestureState.RelativeProgress = NormalizeProgress(e.TotalX, e.TotalY);

                _layoutStateManager.InterpolateLayout(panGestureState.RelativeProgress);

                break;
            case GestureStatus.Completed:

                if (!(panGestureState.Id == e.GestureId && panGestureState.Locked))
                {
                    return;
                }

                if (panGestureState.RelativeProgress >= panGestureState.PanThreshold)
                {
                    await _layoutStateManager.AnimateToNextState();

                    SwitchLayoutState();

                    subtitlesHidden = !subtitlesHidden;
                }
                else
                {
                    await _layoutStateManager.AnimateToCurrentState();
                }

                panGestureState = new();

                break;
        }
    }

    private void RefreshLayoutStates()
    {
        _layoutStateManager.SaveCurrentState();

        if (subtitlesHidden)
        {
            _layoutStateManager.SetNextState(
                [_layoutSettings.PlayerVerticalLength, _layoutSettings.SubtitlesVerticalLength],
                [_layoutSettings.PlayerHorizontalLength, _layoutSettings.SubtitlesHoritzontalLength]
            );
        }
        else
        {
            _layoutStateManager.SetNextState(
                [1, _layoutSettings.SubtitlesVerticalLength],
                [1, _layoutSettings.SubtitlesHoritzontalLength]
            );
        }
    }

    private void SwitchLayoutState()
    {
        if (subtitlesHidden)
        {
            RestoreNormalLayout();
        }
        else
        {
            SetFullscreenLayout();
        }
    }

    private void SetFullscreenLayout()
    {
        AdaptiveLayout.SetRelativeVerticalLength(mauiMediaElement, 1);
        AdaptiveLayout.SetRelativeHorizontalLength(mauiMediaElement, 1);
        AdaptiveLayout.SetRelativeVerticalLength(captionsView, _layoutSettings.SubtitlesVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(captionsView, _layoutSettings.SubtitlesHoritzontalLength);
        mauiMediaElement.ResetTransformations();
        captionsView.ResetTransformations();
    }

    private void RestoreNormalLayout()
    {
        AdaptiveLayout.SetRelativeVerticalLength(mauiMediaElement, _layoutSettings.PlayerVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(mauiMediaElement, _layoutSettings.PlayerHorizontalLength);
        AdaptiveLayout.SetRelativeVerticalLength(captionsView, _layoutSettings.SubtitlesVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(captionsView, _layoutSettings.SubtitlesHoritzontalLength);
        mauiMediaElement.ResetTransformations();
        captionsView.ResetTransformations();
    }

    private double NormalizeProgress(double totalX, double totalY)
    {
        var coefficient = subtitlesHidden ? -1d : 1d;

        var absoluteProgress = IsVerticalLayout ? totalY : totalX;

        var progress = coefficient * absoluteProgress / captionsView.Height;

        return Math.Clamp(progress, 0, 1);
    }

    #endregion
}
