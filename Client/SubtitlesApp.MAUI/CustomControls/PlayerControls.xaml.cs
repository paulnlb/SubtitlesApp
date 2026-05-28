using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using SubtitlesApp.Messages;

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

    public static readonly BindableProperty PositionChangedCommandProperty = BindableProperty.Create(
        nameof(PositionChangedCommand),
        typeof(ICommand),
        typeof(PlayerControls),
        null
    );

    public static readonly BindableProperty PositionChangedCommandParameterProperty = BindableProperty.Create(
        nameof(PositionChangedCommandParameter),
        typeof(object),
        typeof(PlayerControls),
        null
    );

    public static readonly BindableProperty SeekCompletedCommandProperty = BindableProperty.Create(
        nameof(SeekCompletedCommand),
        typeof(ICommand),
        typeof(PlayerControls),
        null
    );

    public static readonly BindableProperty SeekCompletedCommandParameterProperty = BindableProperty.Create(
        nameof(SeekCompletedCommandParameter),
        typeof(object),
        typeof(PlayerControls),
        null
    );

    public MediaElement MauiMediaElement
    {
        get => (MediaElement)GetValue(MauiMediaElementProperty);
        set => SetValue(MauiMediaElementProperty, value);
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
            StrongReferenceMessenger.Default.Unregister<SeekToPositionMessage>(this);
        }

        _disposed = true;
    }

    #endregion

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

    #endregion

    #region private event handlers

    private static void OnMauiMediaElementPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var root = ((PlayerControls)bindable);
        if (oldValue is MediaElement oldMediaElement)
        {
            oldMediaElement.PropertyChanged -= root.MediaElementPropertyChanged;
            oldMediaElement.Handler?.DisconnectHandler();
            oldMediaElement.Dispose();
            StrongReferenceMessenger.Default.Unregister<SeekToPositionMessage>(root);
        }
        if (newValue is MediaElement newMediaElement)
        {
            newMediaElement.PropertyChanged += root.MediaElementPropertyChanged;
            StrongReferenceMessenger.Default.Register<PlayerControls, SeekToPositionMessage>(
                root,
                (recipient, message) => newMediaElement.SeekTo(message.Value)
            );
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

    private void OnFullScreenClicked(object? sender, EventArgs e)
    {
        fullscreenBtn.IsToggled = !fullscreenBtn.IsToggled;
        immersiveBtn.IsToggled = fullscreenBtn.IsToggled;

        FullScreenToggled?.Invoke(this, new StateBtnEventArgs { IsToggled = fullscreenBtn.IsToggled });
        ImmersiveModeToggled?.Invoke(this, new StateBtnEventArgs { IsToggled = immersiveBtn.IsToggled });
    }

    private void OnImmersiveClicked(object? sender, EventArgs e)
    {
        immersiveBtn.IsToggled = !immersiveBtn.IsToggled;
        ImmersiveModeToggled?.Invoke(this, new StateBtnEventArgs { IsToggled = immersiveBtn.IsToggled });
    }

    private void OnPositionChanged(object? sender, EventArgs e)
    {
        if (PositionChangedCommand != null && PositionChangedCommand.CanExecute(PositionChangedCommandParameter))
            PositionChangedCommand.Execute(PositionChangedCommandParameter);
    }

    private void OnSeekCompleted(object? sender, EventArgs e)
    {
        if (SeekCompletedCommand != null && SeekCompletedCommand.CanExecute(SeekCompletedCommandParameter))
            SeekCompletedCommand.Execute(SeekCompletedCommandParameter);
    }
    #endregion
}

public class StateBtnEventArgs : EventArgs
{
    public bool IsToggled { get; set; }
}
