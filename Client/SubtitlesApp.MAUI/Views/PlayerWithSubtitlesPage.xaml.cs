using System.ComponentModel;
using CommunityToolkit.Maui.Views;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.EventArgs;
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

    public PlayerWithSubtitlesPage(PlayerWithSubtitlesViewModel viewModel, ILayoutSettings layoutSettings)
    {
        InitializeComponent();

        BindingContext = viewModel;

        _layoutSettings = layoutSettings;
        _layoutStateManager = new AdaptiveLayoutStateManager(adaptiveLayout);

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;

        viewModel.SubsScrollRequested += OnSubScrollRequested;
        viewModel.TranslationsScrollRequested += OnTranslationScrollRequested;
        viewModel.SeekRequested += OnSeekRequested;
        viewModel.PropertyChanged += OnVmPropertyChanged;
        mauiMediaElement.PropertyChanged += OnMediaPlayerPropertyChanged;
        adaptiveLayout.PropertyChanged += OnLayoutPropertyChanged;

        mauiMediaElement.SetBinding(
            MediaElement.DurationProperty,
            new Binding(nameof(viewModel.MediaDuration), BindingMode.OneWayToSource, source: viewModel)
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
        Vm.SubsScrollRequested -= OnSubScrollRequested;
        Vm.TranslationsScrollRequested -= OnTranslationScrollRequested;
        Vm.SeekRequested -= OnSeekRequested;
        Vm.PropertyChanged -= OnVmPropertyChanged;
        Vm.SubtitlesAdapter.Dispose();
        Vm.TranslationsAdapter.Dispose();
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
            && IsVerticalLayout
        )
        {
            RecalculateVerticalLayout(mauiMediaElement.MediaHeight, mauiMediaElement.MediaWidth);
        }
    }

    private void OnPositionChanged(object? sender, EventArgs e)
    {
        if (Vm.PositionChangedCommand != null && Vm.PositionChangedCommand.CanExecute(mauiMediaElement.Position))
            Vm.PositionChangedCommand.Execute(mauiMediaElement.Position);
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
        AdaptiveLayout.SetRelativeVerticalLength(subtitlesView, _layoutSettings.SubtitlesVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(subtitlesView, _layoutSettings.SubtitlesHoritzontalLength);
        mauiMediaElement.ResetTransformations();
        subtitlesView.ResetTransformations();
    }

    private void RestoreNormalLayout()
    {
        AdaptiveLayout.SetRelativeVerticalLength(mauiMediaElement, _layoutSettings.PlayerVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(mauiMediaElement, _layoutSettings.PlayerHorizontalLength);
        AdaptiveLayout.SetRelativeVerticalLength(subtitlesView, _layoutSettings.SubtitlesVerticalLength);
        AdaptiveLayout.SetRelativeHorizontalLength(subtitlesView, _layoutSettings.SubtitlesHoritzontalLength);
        mauiMediaElement.ResetTransformations();
        subtitlesView.ResetTransformations();
    }

    private double NormalizeProgress(double totalX, double totalY)
    {
        var coefficient = subtitlesHidden ? -1d : 1d;

        var absoluteProgress = IsVerticalLayout ? totalY : totalX;

        var progress = coefficient * absoluteProgress / subtitlesView.Height;

        return Math.Clamp(progress, 0, 1);
    }

    #endregion
}
