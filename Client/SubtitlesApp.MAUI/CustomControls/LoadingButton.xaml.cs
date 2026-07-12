using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Helpers;

namespace SubtitlesApp.CustomControls;

public partial class LoadingButton : ContentView
{
    private IDisposable? _runningAnimation;

    public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create(
        nameof(IsLoading),
        typeof(bool),
        typeof(LoadingButton),
        false,
        propertyChanged: OnIsLoadingChanged
    );

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(LoadingButton),
        null
    );

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(LoadingButton),
        null
    );

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(LoadingButton),
        string.Empty
    );

    public static readonly BindableProperty SpinnerColorProperty = BindableProperty.Create(
        nameof(SpinnerColor),
        typeof(Color),
        typeof(LoadingButton),
        Colors.White
    );

    public static readonly BindableProperty ButtonStyleProperty = BindableProperty.Create(
        nameof(ButtonStyle),
        typeof(Style),
        typeof(LoadingButton),
        null
    );

    public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
        nameof(ImageSource),
        typeof(ImageSource),
        typeof(LoadingButton),
        null
    );

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Color SpinnerColor
    {
        get => (Color)GetValue(SpinnerColorProperty);
        set => SetValue(SpinnerColorProperty, value);
    }

    public Style ButtonStyle
    {
        get => (Style)GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }

    public ImageSource ImageSource
    {
        get => (ImageSource)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public LoadingButton()
    {
        InitializeComponent();
    }

    private static void OnIsLoadingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not LoadingButton loadingBtn || newValue is not bool isLoading)
        {
            return;
        }

        if (isLoading)
        {
            loadingBtn.BtnBorder.StrokeThickness = 2;
            loadingBtn.StartLoadingAnimation();
        }
        else
        {
            loadingBtn.BtnBorder.StrokeThickness = 0;
            loadingBtn._runningAnimation?.Dispose();
        }
    }

    private void StartLoadingAnimation()
    {
        _runningAnimation = NativeAnimation.Animate(
            0,
            1,
            AnimateGradient,
            length: 1500,
            easing: Easing.Linear,
            repeat: () => true
        );
    }

    private void AnimateGradient(double progress)
    {
        var angle = progress * 2 * Math.PI;

        const double centerX = 0.5;
        const double centerY = 0.5;

        const double halfX = 0.5;
        const double halfY = 0.5;

        var dx = halfX * Math.Cos(angle) - halfY * Math.Sin(angle);
        var dy = halfX * Math.Sin(angle) + halfY * Math.Cos(angle);

        LoadingBrush.StartPoint = new Point(centerX - dx, centerY - dy);
        LoadingBrush.EndPoint = new Point(centerX + dx, centerY + dy);
    }

    private void OnCancelBtnClicked(object sender, EventArgs e)
    {
        if (Command is IAsyncRelayCommand asyncCommand)
        {
            asyncCommand.Cancel();
        }
    }
}
