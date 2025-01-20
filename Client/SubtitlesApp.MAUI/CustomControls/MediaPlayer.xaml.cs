using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;

namespace SubtitlesApp.CustomControls;

public partial class MediaPlayer : ContentView
{
    public MediaPlayer()
    {
        InitializeComponent();

        MauiMediaElement.BindingContext = this;
        MauiMediaElement.SetBinding(MediaElement.PositionProperty, "Position", BindingMode.OneWayToSource);
        MauiMediaElement.SetBinding(MediaElement.DurationProperty, "Duration", BindingMode.OneWayToSource);
        MauiMediaElement.SetBinding(MediaElement.MediaWidthProperty, "MediaWidth", BindingMode.OneWayToSource);
        MauiMediaElement.SetBinding(MediaElement.MediaHeightProperty, "MediaHeight", BindingMode.OneWayToSource);

        MauiMediaElement.PropertyChanged += MediaElementPropertyChanged;
    }

    public static readonly BindableProperty MediaPathProperty = BindableProperty.Create(
        nameof(MediaPath),
        typeof(string),
        typeof(MediaPlayer),
        string.Empty
    );

    public static readonly BindableProperty PlayerControlsVisibleProperty = BindableProperty.Create(
        nameof(PlayerControlsVisible),
        typeof(bool),
        typeof(MediaPlayer),
        true
    );

    public static readonly BindableProperty PositionChangedCommandProperty = BindableProperty.Create(
        nameof(PositionChangedCommand),
        typeof(ICommand),
        typeof(MediaPlayer),
        null
    );

    public static readonly BindableProperty PositionChangedCommandParameterProperty = BindableProperty.Create(
        nameof(PositionChangedCommandParameter),
        typeof(object),
        typeof(MediaPlayer),
        null
    );

    public static readonly BindableProperty SeekCompletedCommandProperty = BindableProperty.Create(
        nameof(SeekCompletedCommand),
        typeof(ICommand),
        typeof(MediaPlayer),
        null
    );

    public static readonly BindableProperty SeekCompletedCommandParameterProperty = BindableProperty.Create(
        nameof(SeekCompletedCommandParameter),
        typeof(object),
        typeof(MediaPlayer),
        null
    );

    public static readonly BindableProperty DurationProperty = BindableProperty.Create(
        nameof(Duration),
        typeof(TimeSpan),
        typeof(MediaPlayer),
        TimeSpan.Zero,
        BindingMode.OneWayToSource
    );

    public static readonly BindableProperty PositionProperty = BindableProperty.Create(
        nameof(Position),
        typeof(TimeSpan),
        typeof(MediaPlayer),
        TimeSpan.Zero,
        BindingMode.OneWayToSource,
        propertyChanged: OnPositionChanged
    );

    public static readonly BindableProperty SeekThresholdProperty = BindableProperty.Create(
        nameof(SeekThreshold),
        typeof(TimeSpan),
        typeof(MediaPlayer),
        TimeSpan.FromMilliseconds(20)
    );

    public static readonly BindableProperty MediaWidthProperty = BindableProperty.Create(
        nameof(MediaWidth),
        typeof(int),
        typeof(MediaPlayer),
        0
    );

    public static readonly BindableProperty MediaHeightProperty = BindableProperty.Create(
        nameof(MediaHeight),
        typeof(int),
        typeof(MediaPlayer),
        0
    );

    public string MediaPath
    {
        get => (string)GetValue(MediaPathProperty);
        set => SetValue(MediaPathProperty, value);
    }

    public int MediaWidth
    {
        get => (int)GetValue(MediaWidthProperty);
        set => SetValue(MediaWidthProperty, value);
    }

    public int MediaHeight
    {
        get => (int)GetValue(MediaHeightProperty);
        set => SetValue(MediaHeightProperty, value);
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

    public TimeSpan SeekThreshold
    {
        get => (TimeSpan)GetValue(SeekThresholdProperty);
        set => SetValue(SeekThresholdProperty, value);
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

    private static async void OnPositionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not MediaPlayer mediaPlayer)
        {
            return;
        }

        if (newValue is not TimeSpan newPosition)
        {
            return;
        }

        var positionDifferenceMs = Math.Abs((mediaPlayer.MauiMediaElement.Position - newPosition).TotalMilliseconds);

        if (positionDifferenceMs >= mediaPlayer.SeekThreshold.TotalMilliseconds)
        {
            await mediaPlayer.SeekTo(newPosition);
        }
    }

    private async void OnRewindTapped(object sender, EventArgs e)
    {
        var newPosition = MauiMediaElement.Position.Subtract(TimeSpan.FromSeconds(5));

        if (newPosition < TimeSpan.Zero)
        {
            newPosition = TimeSpan.Zero;
        }

        await SeekTo(newPosition);
    }

    private void OnPlayPauseTapped(object sender, EventArgs e)
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

    private async void OnFastForwardTapped(object sender, EventArgs e)
    {
        var newPosition = MauiMediaElement.Position.Add(TimeSpan.FromSeconds(5));

        if (newPosition > MauiMediaElement.Duration)
        {
            newPosition = MauiMediaElement.Duration;
        }

        await SeekTo(newPosition);
    }

    private void OnDragStarted(object sender, EventArgs e)
    {
        MauiMediaElement.Pause();
    }

    private async void OnDragCompleted(object sender, EventArgs e)
    {
        var seekToTime = TimeSpan.FromSeconds(PositionSlider.Value);

        await MauiMediaElement.SeekTo(seekToTime, CancellationToken.None);
        MauiMediaElement.Play();
    }

    // This event handler exists because of https://github.com/dotnet/maui/issues/12285
    private void MediaElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MauiMediaElement.Position))
        {
            PositionSlider.Value = MauiMediaElement.Position.TotalSeconds;
        }
        else if (e.PropertyName == nameof(MauiMediaElement.Duration))
        {
            PositionSlider.Maximum = MauiMediaElement.Duration.TotalSeconds;
        }
    }
    #endregion
}
