using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;
using SubtitlesApp.Core.Models;
using SubtitlesApp.ViewModels;
using System.ComponentModel;

namespace SubtitlesApp.Views;

public partial class MediaElementPage : ContentPage
{
    readonly ILogger logger;

    readonly MediaElementViewModel _viewModel;

    public MediaElementPage(MediaElementViewModel viewModel, ILogger<MediaElementPage> logger)
    {
        InitializeComponent();

        BindingContext = viewModel;
        _viewModel = viewModel;

        this.logger = logger;
        MediaElement.PropertyChanged += MediaElement_PropertyChanged;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    void MediaElement_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MediaElement.DurationProperty.PropertyName)
        {
            logger.LogInformation("Duration: {newDuration}", MediaElement.Duration);
            PositionSlider.Maximum = MediaElement.Duration.TotalSeconds;
            _viewModel.MediaDuration = MediaElement.Duration;
        }
    }

    void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var vm = sender as MediaElementViewModel;

        if (e.PropertyName == nameof(vm.CurrentSubtitle))
        {
            if (vm?.CurrentSubtitle != null)
            {
                SubsCollection.ScrollTo(vm.CurrentSubtitle);
            }
        }
    }

    async void OnMediaOpened(object? sender, EventArgs e)
    {
       
    }

    void OnStateChanged(object? sender, MediaStateChangedEventArgs e) =>
        logger.LogInformation("Media State Changed. Old State: {PreviousState}, New State: {NewState}", e.PreviousState, e.NewState);

    void OnMediaFailed(object? sender, MediaFailedEventArgs e)
    {
    }

    void OnMediaEnded(object? sender, EventArgs e)
    {
    }

    void OnPositionChanged(object? sender, MediaPositionChangedEventArgs e)
    {
        var currentPosition = e.Position;
        var transcribeCommand = _viewModel.TranscribeCommand;

        (var shouldTranscribe, var transcribeStartTime) = _viewModel.ShouldTranscribe(currentPosition);

        if (shouldTranscribe && transcribeStartTime != null)
        {
            transcribeCommand.Execute(transcribeStartTime);
        }
        else
        {
            _viewModel.SetCurrentSub(currentPosition);
        }

        PositionSlider.Value = currentPosition.TotalSeconds;
    }

    void OnSeekCompleted(object? sender, EventArgs e)
    {
        
    }

    void OnPlayClicked(object? sender, EventArgs e)
    {
        MediaElement.Play();
    }

    void OnPauseClicked(object? sender, EventArgs e)
    {
        MediaElement.Pause();
    }

    void OnStopClicked(object? sender, EventArgs e)
    {
        MediaElement.Stop();
    }

    void OnMuteClicked(object? sender, EventArgs e)
    {
        MediaElement.ShouldMute = !MediaElement.ShouldMute;
    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        await _viewModel.CleanAsync();
        MediaElement.Stop();
        MediaElement.Handler?.DisconnectHandler();
    }

    async void Slider_DragCompleted(object? sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);

        _viewModel.TextBoxContent = "Ready.";

        var transcribeCommand = _viewModel.TranscribeCommand;

        if (transcribeCommand.IsRunning && transcribeCommand.CanBeCanceled)
        {
            transcribeCommand.Cancel();
        }

        var newValue = ((Slider)sender).Value;
        await MediaElement.SeekTo(TimeSpan.FromSeconds(newValue), CancellationToken.None);

        if (MediaElement.CurrentState == MediaElementState.Paused)
        {
            MediaElement.Play();
        }
    }

    void Slider_DragStarted(object sender, EventArgs e)
    {
        MediaElement.Pause();
    }

    async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Subtitle subtitle)
        {
            await MediaElement.SeekTo(subtitle.TimeInterval.StartTime, CancellationToken.None);

            SubsCollection.SelectedItem = null;
        }
    }
}