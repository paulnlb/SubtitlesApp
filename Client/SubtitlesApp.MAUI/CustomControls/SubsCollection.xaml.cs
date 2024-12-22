using SubtitlesApp.Core.Extensions;
using System.Collections.ObjectModel;
using SubtitlesApp.ClientModels;

namespace SubtitlesApp.CustomControls;

public partial class SubsCollection : ContentView
{
    private int _currentSubIndex = 0;
    private bool _autoScrollEnabled = true;

	public SubsCollection()
	{
		InitializeComponent();
    }

    public static readonly BindableProperty SubsSourceProperty =
            BindableProperty.Create(nameof(SubsSource), typeof(ObservableCollection<VisualSubtitle>), typeof(SubsCollection), new ObservableCollection<VisualSubtitle>());

    public static readonly BindableProperty CurrentTimePositionProperty = 
            BindableProperty.Create(nameof(CurrentTimePosition), typeof(TimeSpan), typeof(SubsCollection), TimeSpan.Zero, propertyChanged: OnCurrentTimePositionChanged);

    public static readonly BindableProperty IsScrollButtonVisibleProperty = 
        BindableProperty.Create(nameof(IsScrollButtonVisible), typeof(bool), typeof(SubsCollection), false);

    public static readonly BindableProperty ItemTemplateProperty = 
        BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(SubsCollection), null);

    public ObservableCollection<VisualSubtitle> SubsSource
	{
        get => (ObservableCollection<VisualSubtitle>)GetValue(SubsSourceProperty);
        set => SetValue(SubsSourceProperty, value);
    }

    public TimeSpan CurrentTimePosition
    {
        get => (TimeSpan)GetValue(CurrentTimePositionProperty);
        set => SetValue(CurrentTimePositionProperty, value);
    }

    public bool IsScrollButtonVisible
    {
        get => (bool)GetValue(IsScrollButtonVisibleProperty);
        set => SetValue(IsScrollButtonVisibleProperty, value);
    }

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    VisualSubtitle? GetCurrentSubttle()
    {
        if (SubsSource == null || SubsSource.Count == 0)
        {
            return null;
        }

        return SubsSource[_currentSubIndex];
    }

    void ScrollToIndex(int index)
    {
        subsCollectionView.ScrollTo(index);
    }

    static void OnCurrentTimePositionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not SubsCollection subsCollection)
        {
            return;
        }

        var newPosition = (TimeSpan)newValue;

        var currentSub = subsCollection.GetCurrentSubttle();

        if (currentSub == null)
        {
            return;
        }

        if (currentSub.TimeInterval.ContainsTime(newPosition))
        {
            currentSub.IsHighlighted = true;
            return;
        }

        var (newSub, index) = subsCollection.SubsSource.BinarySearch(newPosition);

        if (newSub != null)
        {
            currentSub.IsHighlighted = false;
            subsCollection._currentSubIndex = index;
            subsCollection.GetCurrentSubttle()!.IsHighlighted = true;

            if (subsCollection._autoScrollEnabled)
            {
                subsCollection.ScrollToIndex(subsCollection._currentSubIndex);
            }
        }
    }

    void OnScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if(e.FirstVisibleItemIndex > _currentSubIndex || e.LastVisibleItemIndex < _currentSubIndex)
        {
            _autoScrollEnabled = false;
            IsScrollButtonVisible = true;
        }
        else
        {
            _autoScrollEnabled = true;
            IsScrollButtonVisible = false;
        }
    }

    void OnSwiped(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem)
        {
            var subtitle = (VisualSubtitle)swipeItem.CommandParameter;
            subtitle?.ApplyTranslation();
        }
    }

    void OnScrollToCurrentClicked(object sender, EventArgs e)
    {
        _autoScrollEnabled = true;

        ScrollToIndex(_currentSubIndex);
    }
}