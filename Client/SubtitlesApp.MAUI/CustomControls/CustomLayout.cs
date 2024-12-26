using MauiPageFullScreen;
using System.ComponentModel;

namespace SubtitlesApp.CustomControls;

public class CustomLayout : ContentView
{
    private View _mainChild;
    private View _sideChild;
    private readonly Grid _grid;

    public static readonly BindableProperty MainChildProperty =
        BindableProperty.Create(nameof(MainChild), typeof(View), typeof(CustomLayout), propertyChanged: OnMainChildChanged);

    public static readonly BindableProperty SideChildProperty =
        BindableProperty.Create(nameof(SideChild), typeof(View), typeof(CustomLayout), propertyChanged: OnSideChildChanged);

    public static readonly BindableProperty MainHeightProperty =
        BindableProperty.Create(nameof(MainHeight), typeof(GridLength), typeof(CustomLayout), new GridLength(1, GridUnitType.Star));

    public static readonly BindableProperty SideHeightProperty =
        BindableProperty.Create(nameof(SideHeight), typeof(GridLength), typeof(CustomLayout), new GridLength(1, GridUnitType.Star));

    public static readonly BindableProperty MainWidthProperty =
        BindableProperty.Create(nameof(MainWidth), typeof(GridLength), typeof(CustomLayout), new GridLength(1, GridUnitType.Star));

    public static readonly BindableProperty SideWidthProperty =
        BindableProperty.Create(nameof(SideWidth), typeof(GridLength), typeof(CustomLayout), new GridLength(1, GridUnitType.Star));

    public static readonly BindableProperty IsSideChildVisibleProperty =
        BindableProperty.Create(nameof(IsSideChildVisible), typeof(bool), typeof(CustomLayout), true, propertyChanged: OnIsSideChildVisibleChanged);

    public static readonly BindableProperty OrientationProperty =
        BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(CustomLayout), MapToStackOrientation(DeviceDisplay.MainDisplayInfo.Orientation), propertyChanged: OnOrientationChanged);

    public View MainChild
    {
        get => (View)GetValue(MainChildProperty);
        set => SetValue(MainChildProperty, value);
    }

    public View SideChild
    {
        get => (View)GetValue(SideChildProperty);
        set => SetValue(SideChildProperty, value);
    }

    [TypeConverter(typeof(GridLengthTypeConverter))]
    public GridLength MainHeight
    {
        get => (GridLength)GetValue(MainHeightProperty);
        set => SetValue(MainHeightProperty, value);
    }

    [TypeConverter(typeof(GridLengthTypeConverter))]
    public GridLength SideHeight
    {
        get => (GridLength)GetValue(SideHeightProperty);
        set => SetValue(SideHeightProperty, value);
    }

    [TypeConverter(typeof(GridLengthTypeConverter))]
    public GridLength MainWidth
    {
        get => (GridLength)GetValue(MainWidthProperty);
        set => SetValue(MainWidthProperty, value);
    }

    [TypeConverter(typeof(GridLengthTypeConverter))]
    public GridLength SideWidth
    {
        get => (GridLength)GetValue(SideWidthProperty);
        set => SetValue(MainWidthProperty, value);
    }

    public bool IsSideChildVisible
    {
        get => (bool)GetValue(IsSideChildVisibleProperty);
        set => SetValue(IsSideChildVisibleProperty, value);
    }

    public StackOrientation Orientation
    {
        get => (StackOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public CustomLayout()
    {
        _grid = new Grid();
        Content = _grid;

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
        UpdateLayout(Orientation);
    }

    public void Unsubscribe()
    {
        DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
    }

    private static void OnMainChildChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var layout = (CustomLayout)bindable;
        layout._mainChild = (View)newValue;

        layout._grid.Children.Add(layout._mainChild);
        layout.UpdateLayout(layout.Orientation);
    }

    private static void OnSideChildChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var layout = (CustomLayout)bindable;
        layout._sideChild = (View)newValue;

        // bind IsVisibleProperty
        layout._sideChild.SetBinding(IsVisibleProperty, new Binding(nameof(IsSideChildVisible), source: layout, mode: BindingMode.OneWay));

        layout._grid.Children.Add(layout._sideChild);
        layout.UpdateLayout(layout.Orientation);
    }

    private static void OnIsSideChildVisibleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var layout = (CustomLayout)bindable;
        layout.UpdateLayout(layout.Orientation);
    }

    private static void OnOrientationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var oldOrientation = (StackOrientation)oldValue;
        var newOrientation = (StackOrientation)newValue;

        if (oldOrientation != newOrientation)
        {
            var layout = (CustomLayout)bindable;

            layout.UpdateLayout(newOrientation);
            layout.ToggleFullScreen(newOrientation);
        }
    }

    private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        Orientation = MapToStackOrientation(e.DisplayInfo.Orientation);
    }

    private void UpdateLayout(StackOrientation orientation)
    {
        _grid.RowDefinitions.Clear();
        _grid.ColumnDefinitions.Clear();

        if (orientation == StackOrientation.Vertical)
        {
            _grid.RowDefinitions.Add(new RowDefinition { Height = IsSideChildVisible ? MainHeight : new GridLength(1, GridUnitType.Star) });
            _grid.RowDefinitions.Add(new RowDefinition { Height = IsSideChildVisible ? SideHeight : new GridLength(0) });

            if (_mainChild != null)
            {
                _grid.SetRow(_mainChild, 0);
            }

            if (_sideChild != null && IsSideChildVisible)
            {
                _grid.SetRow(_sideChild, 1);
            }
        }
        else
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = IsSideChildVisible ? MainWidth : new GridLength(1, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = IsSideChildVisible ? SideWidth : new GridLength(0) });

            if (_mainChild != null)
            {
                _grid.SetColumn(_mainChild, 0);
            }

            if (_sideChild != null && IsSideChildVisible)
            {
                _grid.SetColumn(_sideChild, 1);
            }
        }
    }

    private void ToggleFullScreen(StackOrientation orientation)
    {
        if (orientation == StackOrientation.Horizontal)
        {
            Controls.FullScreen();
        }
        else if (orientation == StackOrientation.Vertical && IsSideChildVisible)
        {
            Controls.RestoreScreen();
        }
    }

    private static StackOrientation MapToStackOrientation(DisplayOrientation orientation)
    {
        return orientation switch
        {
            DisplayOrientation.Portrait => StackOrientation.Vertical,
            DisplayOrientation.Landscape => StackOrientation.Horizontal,
            _ => StackOrientation.Vertical
        };
    }
}