using System.Windows.Input;

namespace SubtitlesApp.CustomControls;

public class ExdendedVirtualList : VirtualListView
{
    public static readonly BindableProperty FocusedItemIndexProperty = BindableProperty.Create(
        nameof(FocusedItemIndex),
        typeof(int),
        typeof(ExdendedVirtualList),
        0,
        propertyChanged: OnFocusedItemIndexChanged
    );

    public static readonly BindableProperty FirstVisibleItemIndexProperty = BindableProperty.Create(
        nameof(FirstVisibleItemIndex),
        typeof(int),
        typeof(ExdendedVirtualList),
        0,
        BindingMode.OneWayToSource
    );

    public static readonly BindableProperty LastVisibleItemIndexProperty = BindableProperty.Create(
        nameof(LastVisibleItemIndex),
        typeof(int),
        typeof(ExdendedVirtualList),
        0,
        BindingMode.OneWayToSource
    );

    public static readonly BindableProperty ScrolledVerticallyCommandProperty = BindableProperty.Create(
        nameof(ScrolledVerticallyCommand),
        typeof(ICommand),
        typeof(ExdendedVirtualList),
        null
    );

    public static readonly BindableProperty AutoScrollEnabledProperty = BindableProperty.Create(
        nameof(AutoScrollEnabled),
        typeof(bool),
        typeof(ExdendedVirtualList),
        true,
        propertyChanged: OnAutoScrollEnabledChanged
    );

    public int FocusedItemIndex
    {
        get => (int)GetValue(FocusedItemIndexProperty);
        set => SetValue(FocusedItemIndexProperty, value);
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

    public bool AutoScrollEnabled
    {
        get => (bool)GetValue(AutoScrollEnabledProperty);
        set => SetValue(AutoScrollEnabledProperty, value);
    }

    public ExdendedVirtualList()
        : base()
    {
        OnScrolled += ElementScrolled;
    }

    ~ExdendedVirtualList()
    {
        OnScrolled -= ElementScrolled;
    }

    private static void OnAutoScrollEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not ExdendedVirtualList extendedCollectionView)
        {
            return;
        }

        if (newValue is not bool autoScrollEnabled)
        {
            return;
        }

        if (autoScrollEnabled)
        {
            extendedCollectionView.ScrollToIndexIfNotVisible(extendedCollectionView.FocusedItemIndex);
        }
    }

    private static void OnFocusedItemIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not ExdendedVirtualList extendedCollectionView)
        {
            return;
        }

        var newIndex = (int)newValue;

        if (newIndex > 0 && extendedCollectionView.AutoScrollEnabled)
        {
            extendedCollectionView.ScrollToIndexIfNotVisible(newIndex);
        }
    }

    private void ElementScrolled(object? sender, ScrolledEventArgs e)
    {
        var visiblePositions = FindVisiblePositions();

        if (visiblePositions.Count == 0)
        {
            return;
        }

        if (visiblePositions[0].ItemIndex != -1)
        {
            FirstVisibleItemIndex = visiblePositions[0].ItemIndex;
        }

        if (visiblePositions[^1].ItemIndex != -1)
        {
            LastVisibleItemIndex = visiblePositions[^1].ItemIndex;
        }

        ScrolledVerticallyCommand.Execute(null);
    }

    private void ScrollToIndexIfNotVisible(int index)
    {
        if (index >= FirstVisibleItemIndex && index <= LastVisibleItemIndex)
        {
            return;
        }

        ScrollToItem(new ItemPosition(0, index), true);
    }
}
