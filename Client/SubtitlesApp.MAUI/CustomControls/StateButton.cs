namespace SubtitlesApp.CustomControls;

public class StateIconButton : Button
{
    public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(
        nameof(IsToggled),
        typeof(bool),
        typeof(StateIconButton),
        false,
        propertyChanged: OnVisualPropertyChanged
    );

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public static readonly BindableProperty State1IconProperty = BindableProperty.Create(
        nameof(State1Icon),
        typeof(ImageSource),
        typeof(StateIconButton),
        propertyChanged: OnVisualPropertyChanged
    );

    public ImageSource State1Icon
    {
        get => (ImageSource)GetValue(State1IconProperty);
        set => SetValue(State1IconProperty, value);
    }

    public static readonly BindableProperty State2IconProperty = BindableProperty.Create(
        nameof(State2Icon),
        typeof(ImageSource),
        typeof(StateIconButton),
        propertyChanged: OnVisualPropertyChanged
    );

    public ImageSource State2Icon
    {
        get => (ImageSource)GetValue(State2IconProperty);
        set => SetValue(State2IconProperty, value);
    }

    public StateIconButton()
    {
        Loaded += OnLoaded;
    }

    ~StateIconButton()
    {
        Loaded -= OnLoaded;
    }

    private void OnLoaded(object? s, EventArgs e)
    {
        UpdateIcon();
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((StateIconButton)bindable).UpdateIcon();
    }

    private void UpdateIcon()
    {
        ImageSource = IsToggled ? State2Icon : State1Icon;
    }
}
