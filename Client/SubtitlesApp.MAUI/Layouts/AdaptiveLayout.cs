using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace SubtitlesApp.Layouts;

public class AdaptiveLayout : Layout
{
    public static readonly BindableProperty RelativeHorizontalLengthProperty =
        BindableProperty.CreateAttached("RelativeHorizontalLength", typeof(double?), typeof(AdaptiveLayout), null, propertyChanged: Invalidate_OnRelativeHorizontalLengthChanged);

    public static readonly BindableProperty RelativeVerticalLengthProperty =
        BindableProperty.CreateAttached("RelativeVerticalLength", typeof(double?), typeof(AdaptiveLayout), null, propertyChanged: Invalidate_OnRelativeVerticalLengthChanged);

    public static double? GetRelativeHorizontalLength(BindableObject view)
    {
        return (double?)view.GetValue(RelativeHorizontalLengthProperty);
    }

    public static void SetRelativeHorizontalLength(BindableObject view, double? value)
    {
        view.SetValue(RelativeHorizontalLengthProperty, value);
    }

    public static double? GetRelativeVerticalLength(BindableObject view)
    {
        return (double?)view.GetValue(RelativeVerticalLengthProperty);
    }

    public static void SetRelativeVerticalLength(BindableObject view, double? value)
    {
        view.SetValue(RelativeVerticalLengthProperty, value);
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

    static void Invalidate_OnRelativeHorizontalLengthChanged(BindableObject bindable, object oldValue, object newValue)
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

    static void Invalidate_OnRelativeVerticalLengthChanged(BindableObject bindable, object oldValue, object newValue)
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

public class DualViewLayoutManager(AdaptiveLayout layout) : ILayoutManager
{
    public Size ArrangeChildren(Rect bounds)
    {
        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            double y = bounds.Y;

            var childrenHeights = GetChildrenHeights(bounds.Height);

            for (int i = 0; i < layout.Count; i++)
            {
                if (layout[i].Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                layout[i].Arrange(new Rect(bounds.X, y, bounds.Width, childrenHeights[i]));

                y += childrenHeights[i];
            }
        }
        else
        {
            double x = bounds.X;

            var childrenWidths = GetChildrenWidths(bounds.Width);

            for (int i = 0; i < layout.Count; i++)
            {
                if (layout[i].Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                layout[i].Arrange(new Rect(x, bounds.Y, childrenWidths[i], bounds.Height));

                x += childrenWidths[i];
            }
        }

        return bounds.Size.AdjustForFill(bounds, layout);
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        double width = 0;
        double height = 0;

        if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            var childrenHeights = GetChildrenHeights(heightConstraint);

            for (int i = 0; i < layout.Count; i++)
            {
                var childSize = layout[i].Measure(widthConstraint, childrenHeights[i]);
                width = Math.Max(width, childSize.Width);
                height += childSize.Height;
            }
        }
        else
        {
            var childrenWidths = GetChildrenWidths(widthConstraint);

            for (int i = 0; i < layout.Count; i++)
            {
                var childSize = layout[i].Measure(childrenWidths[i], heightConstraint);
                width += childSize.Width;
                height = Math.Max(height, childSize.Height);
            }
        }

        return new Size(width, height);
    }

    private List<double> GetChildrenHeights(double totalHeight)
    {
        var relativeLengths = layout.Select(child => AdaptiveLayout.GetRelativeVerticalLength((BindableObject)child)).ToList();
        return GetChildrenAbsoluteLengths(totalHeight, relativeLengths);
    }

    private List<double> GetChildrenWidths(double totalWidth)
    {
        var relativeLengths = layout.Select(child => AdaptiveLayout.GetRelativeHorizontalLength((BindableObject)child)).ToList();
        return GetChildrenAbsoluteLengths(totalWidth, relativeLengths);
    }

    private static List<double> GetChildrenAbsoluteLengths(double totalAbsoluteLength, List<double?> relativeLengths)
    {
        var availableLength = totalAbsoluteLength;
        var nullRelativeLengthsCount = 0;

        foreach (var relativeLength in relativeLengths)
        {
            if (relativeLength.HasValue)
            {
                availableLength -= relativeLength.Value * totalAbsoluteLength;
            }
            else
            {
                nullRelativeLengthsCount++;
            }
        }

        if (availableLength <= 0)
        {
            return relativeLengths.Select(relativeLength =>
            {
                var factor = relativeLength ?? 0;
                return factor * totalAbsoluteLength;
            }).ToList();
        }
        else
        {
            return relativeLengths.Select(relativeLength =>
            {
                if (relativeLength.HasValue)
                {
                    return relativeLength.Value * totalAbsoluteLength;
                }
                else
                {
                    return 1 / nullRelativeLengthsCount * availableLength;
                }
            }).ToList();
        }
    }
}
