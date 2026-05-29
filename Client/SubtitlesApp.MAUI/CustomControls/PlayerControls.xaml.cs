using System.ComponentModel;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace SubtitlesApp.CustomControls;

public partial class PlayerControls : ContentView, IDisposable
{
    private bool _disposed = false;

    public PlayerControls()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty MauiMediaElementProperty = BindableProperty.Create(
        nameof(MauiMediaElement),
        typeof(MediaElement),
        typeof(PlayerControls),
        null,
        propertyChanged: OnMauiMediaElementPropertyChanged
    );

    public static readonly BindableProperty PlayerControlsVisibleProperty = BindableProperty.Create(
        nameof(PlayerControlsVisible),
        typeof(bool),
        typeof(PlayerControls),
        true
    );

    public static readonly BindableProperty IsImmersiveOnProperty = BindableProperty.Create(
        nameof(IsImmersiveOn),
        typeof(bool),
        typeof(PlayerControls),
        false
    );

    public static readonly BindableProperty IsFullScreenOnProperty = BindableProperty.Create(
        nameof(IsFullScreenOn),
        typeof(bool),
        typeof(PlayerControls),
        false
    );

    public MediaElement MauiMediaElement
    {
        get => (MediaElement)GetValue(MauiMediaElementProperty);
        set => SetValue(MauiMediaElementProperty, value);
    }

    public bool PlayerControlsVisible
    {
        get => (bool)GetValue(PlayerControlsVisibleProperty);
        set => SetValue(PlayerControlsVisibleProperty, value);
    }

    public bool IsImmersiveOn
    {
        get => (bool)GetValue(IsImmersiveOnProperty);
        set => SetValue(IsImmersiveOnProperty, value);
    }

    public bool IsFullScreenOn
    {
        get => (bool)GetValue(IsFullScreenOnProperty);
        set => SetValue(IsFullScreenOnProperty, value);
    }

    public event EventHandler<StateBtnEventArgs>? FullScreenToggled;

    public event EventHandler<StateBtnEventArgs>? ImmersiveModeToggled;

    #region implementation of IDisposable

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            MauiMediaElement.PropertyChanged -= MediaElementPropertyChanged;
        }

        _disposed = true;
    }

    #endregion

    #region private event handlers

    private static void OnMauiMediaElementPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var root = ((PlayerControls)bindable);
        if (oldValue is MediaElement oldMediaElement)
        {
            oldMediaElement.PropertyChanged -= root.MediaElementPropertyChanged;
        }
        if (newValue is MediaElement newMediaElement)
        {
            newMediaElement.PropertyChanged += root.MediaElementPropertyChanged;
        }
    }

    private async void OnRewindTapped(object sender, EventArgs e)
    {
        var newPosition = MauiMediaElement.Position.Subtract(TimeSpan.FromSeconds(5));

        if (newPosition < TimeSpan.Zero)
        {
            newPosition = TimeSpan.Zero;
        }

        await MauiMediaElement.SeekTo(newPosition);
    }

    private void OnPlayPauseTapped(object sender, EventArgs e)
    {
        if (MauiMediaElement.CurrentState == MediaElementState.Playing)
        {
            MauiMediaElement.Pause();
        }
        else if (MauiMediaElement.CurrentState == MediaElementState.Stopped)
        {
            MauiMediaElement.SeekTo(TimeSpan.Zero);
            MauiMediaElement.Play();
        }
        else if (MauiMediaElement.CurrentState == MediaElementState.Paused)
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

        await MauiMediaElement.SeekTo(newPosition);
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

    private void OnFullScreenClicked(object? sender, EventArgs e)
    {
        IsFullScreenOn = !IsFullScreenOn;
        FullScreenToggled?.Invoke(this, new StateBtnEventArgs { IsToggled = IsFullScreenOn });
    }

    private void OnImmersiveClicked(object? sender, EventArgs e)
    {
        IsImmersiveOn = !IsImmersiveOn;
        ImmersiveModeToggled?.Invoke(this, new StateBtnEventArgs { IsToggled = IsImmersiveOn });
    }

    #endregion
}

public class StateBtnEventArgs : EventArgs
{
    public bool IsToggled { get; set; }
}
