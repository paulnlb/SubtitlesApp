using System.ComponentModel;
using CommunityToolkit.Maui.Views;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.Enums;
using SubtitlesApp.ClientModels.EventArgs;
using SubtitlesApp.Helpers;
using SubtitlesApp.Layouts;
using SubtitlesApp.Settings;
using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class PlayerWithSubtitlesPage : ContentPage
{
    private bool _subtitlesHidden;
    private bool IsVerticalLayout => adaptiveLayout.ActualOrientation == AdaptiveLayoutOrientation.Vertical;
    private PanGestureState panGestureState = new();
    private readonly AdaptiveLayoutStateManager _layoutStateManager;
    private readonly LayoutSettings _normalLayoutSettings;
    private readonly LayoutSettings _expandedLayoutSettings;

    private PlayerWithSubtitlesViewModel Vm => (PlayerWithSubtitlesViewModel)BindingContext;

    private static readonly BindableProperty LayoutSettingsProperty = BindableProperty.Create(
        nameof(CurrentLayoutSettings),
        typeof(LayoutSettings),
        typeof(PlayerWithSubtitlesPage),
        null
    );

    public LayoutSettings CurrentLayoutSettings
    {
        get => (LayoutSettings)GetValue(LayoutSettingsProperty);
        set => SetValue(LayoutSettingsProperty, value);
    }

    public PlayerWithSubtitlesPage(PlayerWithSubtitlesViewModel vm)
    {
        InitializeComponent();

        _normalLayoutSettings = new(false);
        _expandedLayoutSettings = new(true);
        CurrentLayoutSettings = new(false);

        BindingContext = vm;

        _layoutStateManager = new AdaptiveLayoutStateManager(adaptiveLayout);

        vm.PropertyChanged += OnVmPropertyChanged;
        mauiMediaElement.PropertyChanged += OnMediaPlayerPropertyChanged;
        adaptiveLayout.PropertyChanged += OnLayoutPropertyChanged;

        mauiMediaElement.SetBinding(
            MediaElement.DurationProperty,
            new Binding(nameof(vm.SubtitlesVm.MediaDuration), BindingMode.OneWayToSource, source: vm.SubtitlesVm)
        );

        SubscribeToGestures();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        mauiMediaElement.Stop();
        mauiMediaElement.Handler?.DisconnectHandler();
        mauiMediaElement.Dispose();
        playerControls.Dispose();
        Vm.PropertyChanged -= OnVmPropertyChanged;
        mauiMediaElement.PropertyChanged -= OnMediaPlayerPropertyChanged;
        playerGestureRecognizer.PanUpdated -= HandlePanGesture;
        playerControlsGestureRecognizer.PanUpdated -= HandlePanGesture;
        subtitlesGestureRecognizer.PanUpdated -= HandlePanGesture;
        adaptiveLayout.PropertyChanged -= OnLayoutPropertyChanged;
        subtitlesView.Cleanup();
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

        newRelativeHeight = Math.Min(_normalLayoutSettings.MaxPlayerRelativeVerticalLength, newRelativeHeight);

        _normalLayoutSettings.PlayerVerticalLength = newRelativeHeight;
        _normalLayoutSettings.SubtitlesVerticalLength = 1 - newRelativeHeight;
        _expandedLayoutSettings.SubtitlesVerticalLength = 1 - newRelativeHeight;

        if (!_subtitlesHidden)
        {
            CurrentLayoutSettings.CopyFrom(_normalLayoutSettings);
        }
        else
        {
            CurrentLayoutSettings.CopyFrom(_expandedLayoutSettings);
        }
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
                    await _layoutStateManager.SwitchToNextState();

                    _subtitlesHidden = !_subtitlesHidden;
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

        if (_subtitlesHidden)
        {
            _layoutStateManager.SetNextState(
                [_normalLayoutSettings.PlayerVerticalLength, _normalLayoutSettings.SubtitlesVerticalLength],
                [_normalLayoutSettings.PlayerHorizontalLength, _normalLayoutSettings.SubtitlesHoritzontalLength]
            );
        }
        else
        {
            _layoutStateManager.SetNextState(
                [_expandedLayoutSettings.PlayerVerticalLength, _expandedLayoutSettings.SubtitlesVerticalLength],
                [_expandedLayoutSettings.PlayerHorizontalLength, _expandedLayoutSettings.SubtitlesHoritzontalLength]
            );
        }
    }

    private double NormalizeProgress(double totalX, double totalY)
    {
        var coefficient = _subtitlesHidden ? -1d : 1d;

        var absoluteProgress = IsVerticalLayout ? totalY : totalX;

        var progress = coefficient * absoluteProgress / subtitlesView.Height;

        return Math.Clamp(progress, 0, 1);
    }

    #endregion
}
