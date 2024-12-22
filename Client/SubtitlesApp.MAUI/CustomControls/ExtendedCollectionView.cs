using CommunityToolkit.Maui.Behaviors;
using System.Windows.Input;

namespace SubtitlesApp.CustomControls;

public partial class ExtendedCollectionView : CollectionView
{
    public static readonly BindableProperty ScrollToIndexProperty =
        BindableProperty.Create(nameof(ScrollToIndex), typeof(int), typeof(ExtendedCollectionView), 0, propertyChanged: OnScrollToIndexChanged);

    public static readonly BindableProperty ScrolledCommandProperty =
            BindableProperty.Create(nameof(ScrolledCommand), typeof(ICommand), typeof(ExtendedCollectionView), null);

    public static readonly BindableProperty FirstVisibleItemIndexProperty =
            BindableProperty.Create(nameof(FirstVisibleItemIndex), typeof(int), typeof(ExtendedCollectionView), 0, BindingMode.OneWayToSource);

    public static readonly BindableProperty LastVisibleItemIndexProperty =
            BindableProperty.Create(nameof(LastVisibleItemIndex), typeof(int), typeof(ExtendedCollectionView), 0, BindingMode.OneWayToSource);

    public int ScrollToIndex
    {
        get => (int)GetValue(ScrollToIndexProperty);
        set => SetValue(ScrollToIndexProperty, value);
    }

    public ICommand ScrolledCommand
    {
        get => (ICommand)GetValue(ScrolledCommandProperty);
        set => SetValue(ScrolledCommandProperty, value);
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

    static void OnScrollToIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not ExtendedCollectionView extendedCollectionView)
        {
            return;
        }

        var oldIndex = (int)oldValue;
        var newIndex = (int)newValue;

        if (oldIndex == newIndex)
        {
            return;
        }

        if (newIndex < 0)
        {
            return;
        }

        extendedCollectionView.ScrollTo(newIndex);
    }

    void OnScrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        FirstVisibleItemIndex = e.FirstVisibleItemIndex;
        LastVisibleItemIndex = e.LastVisibleItemIndex;
        ScrolledCommand.Execute(null);
    }
}
