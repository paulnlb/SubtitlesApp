using System.Windows.Input;

namespace SubtitlesApp.CustomControls;

public partial class ExtendedCollectionView : CollectionView
{
    public static readonly BindableProperty ScrollToIndexProperty =
        BindableProperty.Create(nameof(ScrollToIndex), typeof(int), typeof(ExtendedCollectionView), 0, propertyChanged: OnScrollToIndexChanged);

    public static readonly BindableProperty FirstVisibleItemIndexProperty =
            BindableProperty.Create(nameof(FirstVisibleItemIndex), typeof(int), typeof(ExtendedCollectionView), 0, BindingMode.OneWayToSource);

    public static readonly BindableProperty LastVisibleItemIndexProperty =
            BindableProperty.Create(nameof(LastVisibleItemIndex), typeof(int), typeof(ExtendedCollectionView), 0, BindingMode.OneWayToSource);

    public static readonly BindableProperty ScrolledDownCommandProperty =
            BindableProperty.Create(nameof(ScrolledDownCommand), typeof(ICommand), typeof(ExtendedCollectionView), null);

    public static readonly BindableProperty ScrolledUpCommandProperty =
        BindableProperty.Create(nameof(ScrolledUpCommand), typeof(ICommand), typeof(ExtendedCollectionView), null);

    public int ScrollToIndex
    {
        get => (int)GetValue(ScrollToIndexProperty);
        set => SetValue(ScrollToIndexProperty, value);
    }

    public ICommand ScrolledUpCommand
    {
        get => (ICommand)GetValue(ScrolledUpCommandProperty);
        set => SetValue(ScrolledUpCommandProperty, value);
    }

    public ICommand ScrolledDownCommand
    {
        get => (ICommand)GetValue(ScrolledDownCommandProperty);
        set => SetValue(ScrolledDownCommandProperty, value);
    }

    public int FirstVisibleItemIndex
    {
        get => (int)GetValue(FirstVisibleItemIndexProperty);
        set => SetValue(FirstVisibleItemIndexProperty, value);
    }

    public int LastVisibleItemIndex
    {
        get => (int)GetValue(LastVisibleItemIndexProperty);
        set => SetValue(LastVisibleItemIndexProperty, value);
    }

    public ExtendedCollectionView(): base()
    {
        Scrolled += OnScrolled;
    }

    ~ExtendedCollectionView()
    {
        Scrolled -= OnScrolled;
    }

    static void OnScrollToIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not ExtendedCollectionView extendedCollectionView)
        {
            return;
        }

        var newIndex = (int)newValue;

        if (newIndex < 0)
        {
            return;
        }

        extendedCollectionView.ScrollTo(newIndex, position: ScrollToPosition.End);
    }

    void OnScrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        FirstVisibleItemIndex = e.FirstVisibleItemIndex;
        LastVisibleItemIndex = e.LastVisibleItemIndex;

        if (e.VerticalDelta > 0)
        {
            ScrolledDownCommand.Execute(null);
        }
        else
        {
            ScrolledUpCommand.Execute(null);
        }
    }
}
