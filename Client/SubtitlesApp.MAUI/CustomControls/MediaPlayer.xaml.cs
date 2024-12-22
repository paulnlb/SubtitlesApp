using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
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

    public static readonly BindableProperty MediaPathProperty =
            BindableProperty.Create(nameof(MediaPath), typeof(string), typeof(MediaPlayer), string.Empty);

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
            BindableProperty.Create(nameof(Duration), typeof(TimeSpan), typeof(MediaPlayer), TimeSpan.Zero, BindingMode.OneWayToSource);

    public static readonly BindableProperty PositionProperty =
            BindableProperty.Create(nameof(Position), typeof(TimeSpan), typeof(MediaPlayer), TimeSpan.Zero, BindingMode.OneWayToSource);

    public static readonly BindableProperty PositionToSeekProperty =
            BindableProperty.Create(nameof(PositionToSeek), typeof(TimeSpan), typeof(MediaPlayer), TimeSpan.Zero, propertyChanged: OnPositionToSeekChanged);



    public string MediaPath
    {
        get => (string)GetValue(MediaPathProperty);
        set => SetValue(MediaPathProperty, value);
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
        set => SetValue(DurationProperty, value);
    }

    public TimeSpan Position
    {
        get => (TimeSpan)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public TimeSpan PositionToSeek
    {
        get => (TimeSpan)GetValue(PositionToSeekProperty);
        set => SetValue(PositionToSeekProperty, value);
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

    public async Task SeekTo(TimeSpan position, CancellationToken cancellationToken = default)
    {
        await MauiMediaElement.SeekTo(position, cancellationToken);
    }

    public void DisconnectHandler()
    {
        MauiMediaElement.PropertyChanged -= MediaElementPropertyChanged;
        MauiMediaElement.Handler?.DisconnectHandler();
    }

    #endregion

    #region private event handlers

    static async void OnPositionToSeekChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not MediaPlayer mediaPlayer)
        {
            return;
        }

        if (oldValue == newValue || 
            oldValue is not TimeSpan || 
            newValue is not TimeSpan newPosition)
        { 
            return;
        }

        await mediaPlayer.SeekTo(newPosition);
    }

    async void OnRewindTapped(object sender, EventArgs e)
    {
        var newPosition = MauiMediaElement.Position.Subtract(TimeSpan.FromSeconds(5));

        if (newPosition < TimeSpan.Zero)
        {
            newPosition = TimeSpan.Zero;
        }

        await SeekTo(newPosition);
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

        await SeekTo(newPosition);
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