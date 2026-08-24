using SubtitlesApp.ClientModels;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.CustomControls;

public partial class TimeSelector : ContentView
{
    private readonly ICustomPopupService _popupService;

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(TimeSelector),
        default(string)
    );

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(TimeSpan),
        typeof(TimeSelector),
        default(TimeSpan)
    );

    public TimeSpan Value
    {
        get => (TimeSpan)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly BindableProperty MinValueProperty = BindableProperty.Create(
        nameof(MinValue),
        typeof(TimeSpan),
        typeof(TimeSelector),
        default(TimeSpan)
    );

    public TimeSpan MinValue
    {
        get => (TimeSpan)GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public static readonly BindableProperty MaxValueProperty = BindableProperty.Create(
        nameof(MaxValue),
        typeof(TimeSpan),
        typeof(TimeSelector),
        TimeSpan.MaxValue
    );

    public TimeSpan MaxValue
    {
        get => (TimeSpan)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly BindableProperty PresetsProperty = BindableProperty.Create(
        nameof(Presets),
        typeof(IEnumerable<TimePreset>),
        typeof(TimeSelector),
        null
    );

    public IEnumerable<TimePreset> Presets
    {
        get => (IEnumerable<TimePreset>)GetValue(PresetsProperty);
        set => SetValue(PresetsProperty, value);
    }

    public TimeSelector()
    {
        InitializeComponent();

        var popupService = IPlatformApplication.Current?.Services.GetRequiredService<ICustomPopupService>();

        if (popupService is null)
        {
            throw new ArgumentNullException(nameof(popupService));
        }

        _popupService = popupService;
    }

    private async void OnTapped(object sender, EventArgs e)
    {
        var result = await _popupService.ShowTimeEntry(Title, Value, MinValue, MaxValue, timePresets: Presets);

        if (result is TimeSpan selectedTime)
        {
            Value = selectedTime;
        }
    }
}
