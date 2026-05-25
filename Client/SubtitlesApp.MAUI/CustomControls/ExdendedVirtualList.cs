using System.Windows.Input;

namespace SubtitlesApp.CustomControls;

public class ExdendedVirtualList : VirtualListView
{
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

    public static readonly BindableProperty ScrolledCommandDerivedProperty = BindableProperty.Create(
        nameof(ScrolledCommandDerived),
        typeof(ICommand),
        typeof(ExdendedVirtualList),
        null
    );

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

    // Executed after the ElementScrolled updated visible intexes
    public ICommand? ScrolledCommandDerived
    {
        get => (ICommand?)GetValue(ScrolledCommandDerivedProperty);
        set => SetValue(ScrolledCommandDerivedProperty, value);
    }

    public ExdendedVirtualList()
        : base()
    {
        OnScrolled += ElementScrolled;
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

        if (ScrolledCommandDerived != null && ScrolledCommandDerived.CanExecute(e))
            ScrolledCommandDerived.Execute(e);
    }

    public void ScrollToIndex(int index)
    {
        ScrollToItem(new ItemPosition(0, index), true);
    }
}
