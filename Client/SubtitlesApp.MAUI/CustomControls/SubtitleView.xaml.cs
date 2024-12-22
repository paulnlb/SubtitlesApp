using SubtitlesApp.ClientModels;
using System.Windows.Input;

namespace SubtitlesApp.CustomControls;

public partial class SubtitleView : ContentView
{
    public static readonly BindableProperty SubtitleSourceProperty =
        BindableProperty.Create(
            nameof(SubtitleSource),
            typeof(VisualSubtitle),
            typeof(SubtitleView),
            default(VisualSubtitle),
            propertyChanged: OnSubtitleSourceChanged);

    public static readonly BindableProperty TappedCommandProperty =
        BindableProperty.Create(nameof(TappedCommand), typeof(ICommand), typeof(SubtitleView));

    public static readonly BindableProperty TappedCommandParameterProperty =
        BindableProperty.Create(nameof(TappedCommandParameter), typeof(object), typeof(SubtitleView));

    public SubtitleView()
    {
        InitializeComponent();
    }

    public VisualSubtitle SubtitleSource
    {
        get => (VisualSubtitle)GetValue(SubtitleSourceProperty);
        set => SetValue(SubtitleSourceProperty, value);
    }

    public ICommand TappedCommand
    {
        get => (ICommand)GetValue(TappedCommandProperty);
        set => SetValue(TappedCommandProperty, value);
    }

    public object TappedCommandParameter
    {
        get => GetValue(TappedCommandParameterProperty);
        set => SetValue(TappedCommandParameterProperty, value);
    }

    private static void OnSubtitleSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SubtitleView subtitleView)
        {
            subtitleView.BindingContext = newValue;
        }
    }

    void OnSwiped(object sender, EventArgs e)
    {
        SubtitleSource?.ApplyTranslation();
    }
}