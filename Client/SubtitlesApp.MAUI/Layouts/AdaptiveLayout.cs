using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace SubtitlesApp.Layouts;

public class AdaptiveLayout : Layout
{
    public static readonly BindableProperty WidthFactorProperty =
        BindableProperty.CreateAttached("WidthFactor", typeof(double?), typeof(AdaptiveLayout), null, propertyChanged: Invalidate_OnWidthFactorChanged);

    public static readonly BindableProperty HeightFactorProperty =
        BindableProperty.CreateAttached("HeightFactor", typeof(double?), typeof(AdaptiveLayout), null, propertyChanged: Invalidate_OnHeightFactorChanged);

    public static double? GetWidthFactor(BindableObject view)
    {
        return (double?)view.GetValue(WidthFactorProperty);
    }

    public static void SetWidthFactor(BindableObject view, double? value)
    {
        view.SetValue(WidthFactorProperty, value);
    }

    public static double? GetHeightFactor(BindableObject view)
    {
        return (double?)view.GetValue(HeightFactorProperty);
    }

    public static void SetHeightFactor(BindableObject view, double? value)
    {
        view.SetValue(HeightFactorProperty, value);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        Clip = new RectangleGeometry(new Rect(0, 0, Width, Height));

        InvalidateMeasure();
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return new DualViewLayoutManager(this);
    }

    protected override void InvalidateMeasure()
    {
        base.InvalidateMeasure();
        (this as IView).InvalidateMeasure();
    }

    static void Invalidate_OnWidthFactorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Do not invalidate if orientation is vertical and any of width factors changed
        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            return;
        }

        if (bindable is Element element && element.Parent is AdaptiveLayout parentLayout)
        {
            parentLayout.InvalidateMeasure();
        }
    }

    static void Invalidate_OnHeightFactorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Do not invalidate if orientation is not vertical and any of height factors changed
        if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
            return;
        }

        if (bindable is Element element && element.Parent is AdaptiveLayout parentLayout)
        {
            parentLayout.InvalidateMeasure();
        }
    }
}

public class DualViewLayoutManager : ILayoutManager
{
    private AdaptiveLayout _layout;
    public DualViewLayoutManager(AdaptiveLayout layout)
    {
        _layout = layout;
    }

    public Size ArrangeChildren(Rect bounds)
    {
        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            double y = bounds.Y;

            var childrenHeights = GetChildrenHeights(bounds.Height);

            for (int i = 0; i < _layout.Count; i++)
            {
                if (_layout.Children[i].Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                _layout.Children[i].Arrange(new Rect(bounds.X, y, bounds.Width, childrenHeights[i]));

                y += childrenHeights[i];
            }
        }
        else
        {
            double x = bounds.X;

            var childrenWidths = GetChildrenWidths(bounds.Width);

            for (int i = 0; i < _layout.Count; i++)
            {
                if (_layout.Children[i].Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                _layout.Children[i].Arrange(new Rect(x, bounds.Y, childrenWidths[i], bounds.Height));

                x += childrenWidths[i];
            }
        }

        return bounds.Size.AdjustForFill(bounds, _layout);
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        double width = 0;
        double height = 0;

        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            var childrenHeights = GetChildrenHeights(heightConstraint);

            for (int i = 0; i < _layout.Count; i++)
            {
                var childSize = _layout.Children[i].Measure(widthConstraint, childrenHeights[i]);
                width = Math.Max(width, childSize.Width);
                height += childSize.Height;
            }
        }
        else
        {
            var childrenWidths = GetChildrenWidths(widthConstraint);

            for (int i = 0; i < _layout.Count; i++)
            {
                var childSize = _layout.Children[i].Measure(childrenWidths[i], heightConstraint);
                width += childSize.Width;
                height = Math.Max(height, childSize.Height);
            }
        }

        return new Size(width, height);
    }

    private List<double> GetChildrenHeights(double totalHeight)
    {
        var heightFactors = _layout.Children.Select(child => AdaptiveLayout.GetHeightFactor((BindableObject)child)).ToList();
        var availableHeight = totalHeight;
        var nullHeightFactorsCount = 0;

        foreach (var heightFactor in heightFactors)
        {
            if (heightFactor.HasValue)
            {
                availableHeight -= heightFactor.Value * totalHeight;
            }
            else
            {
                nullHeightFactorsCount++;
            }
        }

        if (availableHeight <= 0)
        {
            return heightFactors.Select(heightFactor =>
            {
                var factor = heightFactor ?? 0;
                return factor * totalHeight;
            }).ToList();
        }
        else
        {
            return heightFactors.Select(heightFactor =>
            {
                if (heightFactor.HasValue)
                {
                    return heightFactor.Value * totalHeight;
                }
                else
                {
                    return 1 / nullHeightFactorsCount * availableHeight;
                }
            }).ToList();
        }
    }

    private List<double> GetChildrenWidths(double totalWidth)
    {
        var widthFactors = _layout.Children.Select(child => AdaptiveLayout.GetWidthFactor((BindableObject)child)).ToList();
        var availableWidth = totalWidth;
        var nullWidthFactorsCount = 0;

        foreach (var widthFactor in widthFactors)
        {
            if (widthFactor.HasValue)
            {
                availableWidth -= widthFactor.Value * totalWidth;
            }
            else
            {
                nullWidthFactorsCount++;
            }
        }

        if (availableWidth <= 0)
        {
            return widthFactors.Select(widthFactor =>
            {
                var factor = widthFactor ?? 0;
                return factor * totalWidth;
            }).ToList();
        }
        else
        {
            return widthFactors.Select(widthFactor =>
            {
                if (widthFactor.HasValue)
                {
                    return widthFactor.Value * totalWidth;
                }
                else
                {
                    return 1 / nullWidthFactorsCount * availableWidth;
                }
            }).ToList();
        }
    }
}
