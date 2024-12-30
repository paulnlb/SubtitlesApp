using System.Windows.Input;

namespace SubtitlesApp.CustomControls;

public class ExdendedVirtualList : VirtualListView
{
    public static readonly BindableProperty ScrollToIndexProperty =
        BindableProperty.Create(nameof(ScrollToIndex), typeof(int), typeof(ExdendedVirtualList), 0, propertyChanged: OnScrollToIndexChanged);

    public static readonly BindableProperty FirstVisibleItemIndexProperty =
            BindableProperty.Create(nameof(FirstVisibleItemIndex), typeof(int), typeof(ExdendedVirtualList), 0, BindingMode.OneWayToSource);

    public static readonly BindableProperty LastVisibleItemIndexProperty =
            BindableProperty.Create(nameof(LastVisibleItemIndex), typeof(int), typeof(ExdendedVirtualList), 0, BindingMode.OneWayToSource);

    public static readonly BindableProperty ScrolledVerticallyCommandProperty =
            BindableProperty.Create(nameof(ScrolledVerticallyCommand), typeof(ICommand), typeof(ExdendedVirtualList), null);

    public int ScrollToIndex
    {
        get => (int)GetValue(ScrollToIndexProperty);
        set => SetValue(ScrollToIndexProperty, value);
    }

    public ICommand ScrolledVerticallyCommand
    {
        get => (ICommand)GetValue(ScrolledVerticallyCommandProperty);
        set => SetValue(ScrolledVerticallyCommandProperty, value);
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

    public ExdendedVirtualList() : base()
    {
        OnScrolled += ElementScrolled;
    }

    ~ExdendedVirtualList()
    {
        OnScrolled -= ElementScrolled;
    }

    static void OnScrollToIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not ExdendedVirtualList extendedCollectionView)
        {
            return;
        }

        var newIndex = (int)newValue;

        if (newIndex < 0)
        {
            return;
        }

        extendedCollectionView.ScrollToItem(new ItemPosition(0, newIndex), true);
    }

    void ElementScrolled(object? sender, ScrolledEventArgs e)
    {
        var visiblePositions = FindVisiblePositions();

        if (visiblePositions.Count == 0)
        {
            return;
        }

        FirstVisibleItemIndex = visiblePositions[0].ItemIndex;
        LastVisibleItemIndex = visiblePositions[^1].ItemIndex;

        ScrolledVerticallyCommand.Execute(null);
    }
}
