using CommunityToolkit.Maui.Core.Primitives;
using System.ComponentModel;
using System.Windows.Input;

namespace SubtitlesApp.CustomControls;

public partial class MediaPlayer : ContentView
{
	public MediaPlayer()
	{
        InitializeComponent();

        MauiMediaElement.PropertyChanged += MediaElementPropertyChanged;
    }

    public static readonly BindableProperty MediaSourceProperty =
            BindableProperty.Create(nameof(MediaSource), typeof(string), typeof(MediaPlayer), string.Empty);

    public static readonly BindableProperty PlayerControlsVisibleProperty =
            BindableProperty.Create(nameof(PlayerControlsVisible), typeof(bool), typeof(MediaPlayer), true);

    public static readonly BindableProperty PositionChangedCommandProperty =
            BindableProperty.Create(nameof(PositionChangedCommand), typeof(ICommand), typeof(MediaPlayer), null);

    public static readonly BindableProperty PositionChangedCommandParameterProperty =
            BindableProperty.Create(nameof(PositionChangedCommandParameter), typeof(object), typeof(MediaPlayer), null);

    public static readonly BindableProperty SeekCompletedCommandProperty =
            BindableProperty.Create(nameof(SeekCompletedCommand), typeof(ICommand), typeof(MediaPlayer), null);

    public static readonly BindableProperty SeekCompletedCommandParameterProperty =
            BindableProperty.Create(nameof(SeekCompletedCommandParameter), typeof(object), typeof(MediaPlayer), null);

    public static readonly BindableProperty DurationProperty =
            BindableProperty.Create(nameof(Duration), typeof(TimeSpan), typeof(MediaPlayer), TimeSpan.Zero);

    public static readonly BindableProperty PositionProperty =
            BindableProperty.Create(nameof(Position), typeof(TimeSpan), typeof(MediaPlayer), TimeSpan.Zero);

    public string MediaSource
    {
        get => (string)GetValue(MediaSourceProperty);
        set => SetValue(MediaSourceProperty, value);
    }

    public ICommand PositionChangedCommand 
    {
        get => (ICommand)GetValue(PositionChangedCommandProperty);
        set => SetValue(PositionChangedCommandProperty, value);
    }

    public object PositionChangedCommandParameter 
    {
        get => GetValue(PositionChangedCommandParameterProperty);
        set => SetValue(PositionChangedCommandParameterProperty, value);
    }

    public ICommand SeekCompletedCommand 
    {
        get => (ICommand)GetValue(SeekCompletedCommandProperty);
        set => SetValue(SeekCompletedCommandProperty, value);
    }

    public object SeekCompletedCommandParameter 
    {
        get => GetValue(SeekCompletedCommandParameterProperty);
        set => SetValue(SeekCompletedCommandParameterProperty, value);
    }

    public bool PlayerControlsVisible
    {
        get => (bool)GetValue(PlayerControlsVisibleProperty);
        set => SetValue(PlayerControlsVisibleProperty, value);
    }

    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
    }

    public TimeSpan Position
    {
        get => (TimeSpan)GetValue(PositionProperty);
    }

    #region public methods
    public void Play()
    {
        MauiMediaElement.Play();
    }

    public void Pause()
    {
        MauiMediaElement.Pause();
    }

    public void Stop()
    {
        MauiMediaElement.Stop();
    }

    public async Task SeekTo(TimeSpan position, CancellationToken cancellationToken)
    {
        await MauiMediaElement.SeekTo(position, cancellationToken);
    }

    public void DisconnectHandler()
    {
        MauiMediaElement.Handler?.DisconnectHandler();
    }

    #endregion

    #region private event handlers

    async void OnRewindTapped(object sender, EventArgs e)
    {
        var newPosition = MauiMediaElement.Position.Subtract(TimeSpan.FromSeconds(5));

        if (newPosition < TimeSpan.Zero)
        {
            newPosition = TimeSpan.Zero;
        }

        await MauiMediaElement.SeekTo(newPosition, CancellationToken.None);
    }

    void OnPlayPauseTapped(object sender, EventArgs e)
    {
        if (MauiMediaElement.CurrentState == MediaElementState.Playing)
        {
            MauiMediaElement.Pause();
        }
        else
        {
            MauiMediaElement.Play();
        }
    }

    async void OnFastForwardTapped(object sender, EventArgs e)
    {
        var newPosition = MauiMediaElement.Position.Add(TimeSpan.FromSeconds(5));

        if (newPosition > MauiMediaElement.Duration)
        {
            newPosition = MauiMediaElement.Duration;
        }

        await MauiMediaElement.SeekTo(newPosition, CancellationToken.None);
    }

    void OnDragStarted(object sender, EventArgs e)
    {
        MauiMediaElement.Pause();
    }

    async void OnDragCompleted(object sender, EventArgs e)
    {
        var seekToTime = TimeSpan.FromSeconds(PositionSlider.Value);

        await MauiMediaElement.SeekTo(seekToTime, CancellationToken.None);
        MauiMediaElement.Play();
    }

    void MediaElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MauiMediaElement.Position))
        {
            PositionSlider.Value = MauiMediaElement.Position.TotalSeconds;
            SetValue(PositionProperty, MauiMediaElement.Position);
        }
        else if (e.PropertyName == nameof(MauiMediaElement.Duration))
        {
            PositionSlider.Maximum = MauiMediaElement.Duration.TotalSeconds;
            SetValue(DurationProperty, MauiMediaElement.Duration);
        }
    }

    #endregion
}