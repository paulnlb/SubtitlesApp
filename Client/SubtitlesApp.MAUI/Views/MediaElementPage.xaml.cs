using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;
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
    }

    void MediaElement_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MediaElement.DurationProperty.PropertyName)
        {
            logger.LogInformation("Duration: {newDuration}", MediaElement.Duration);
            PositionSlider.Maximum = MediaElement.Duration.TotalSeconds;
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
        var subtitle = _viewModel.Subtitles.FirstOrDefault(s => (int)s.StartTime.TotalSeconds == (int)e.Position.TotalSeconds);
        if (subtitle != null && !_viewModel.ShownSubtitles.Contains(subtitle))
        {
            _viewModel.ShownSubtitlesText.Add(subtitle.Text);
            _viewModel.ShownSubtitles.Add(subtitle);
        }

        PositionSlider.Value = e.Position.TotalSeconds;
    }

    void OnSeekCompleted(object? sender, EventArgs e)
    {
        var transcribeCommand = _viewModel.TranscribeCommand;
        var clearCommand = _viewModel.ClearSubtitlesCommand;

        if (transcribeCommand.IsRunning && transcribeCommand.CanBeCanceled)
        {
            transcribeCommand.Cancel();
        }

        clearCommand.Execute(null);

        transcribeCommand.Execute(MediaElement.Position);
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

        var newValue = ((Slider)sender).Value;
        await MediaElement.SeekTo(TimeSpan.FromSeconds(newValue), CancellationToken.None);

        MediaElement.Play();
    }

    void Slider_DragStarted(object sender, EventArgs e)
    {
        MediaElement.Pause();
    }
}