using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace SubtitlesApp.Layouts;

/// <summary>
///     Custom layout that arranges views in one column (in portrait mode) or row (otherwise).
///     <para/>
///     Layout orientation is calculated dynamically during runtime, based on device orientation.
///     <para/>
///     Also this layout allows to specify its items' length relative to parent length.
/// </summary>
public class AdaptiveLayout : Layout
{
    public static readonly BindableProperty RelativeHorizontalLengthProperty = BindableProperty.CreateAttached(
        "RelativeHorizontalLength",
        typeof(double?),
        typeof(AdaptiveLayout),
        null,
        propertyChanged: Invalidate_OnRelativeHorizontalLengthChanged
    );

    public static readonly BindableProperty RelativeVerticalLengthProperty = BindableProperty.CreateAttached(
        "RelativeVerticalLength",
        typeof(double?),
        typeof(AdaptiveLayout),
        null,
        propertyChanged: Invalidate_OnRelativeVerticalLengthChanged
    );

    public static readonly BindableProperty OrientationProperty = BindableProperty.Create(
        nameof(Orientation),
        typeof(StackOrientation),
        typeof(AdaptiveLayout),
        DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait
            ? StackOrientation.Vertical
            : StackOrientation.Horizontal,
        propertyChanged: Invalidate_OnOrientationChanged
    );

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

    public StackOrientation Orientation
    {
        get => (StackOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public AdaptiveLayoutState MakeSnapshot()
    {
        var childrenStates = new List<ChildState>();

        foreach (var child in Children)
        {
            if (child is not VisualElement childElement)
            {
                continue;
            }

            childrenStates.Add(
                new ChildState
                {
                    HorizontalLength = GetRelativeHorizontalLength(childElement) ?? 0,
                    VerticalLength = GetRelativeVerticalLength(childElement) ?? 0,
                    Y = childElement.Y,
                    X = childElement.X,
                    Width = childElement.Width,
                    Height = childElement.Height,
                    TranslationX = childElement.TranslationX,
                    TranslationY = childElement.TranslationY,
                    Scale = childElement.Scale,
                }
            );
        }

        return new AdaptiveLayoutState(childrenStates, Bounds);
    }

    public void Restore(AdaptiveLayoutState layoutState)
    {
        if (layoutState.ChildrenStates.Count != Children.Count)
        {
            throw new ArgumentException(
                $"Layout state contains {layoutState.ChildrenStates.Count} children states, but layout has {Children.Count} children."
            );
        }

        for (int i = 0; i < Children.Count; i++)
        {
            var childState = layoutState.ChildrenStates[i];
            var childView = (VisualElement)Children[i];

            SetRelativeVerticalLength(childView, childState.VerticalLength);
            SetRelativeHorizontalLength(childView, childState.HorizontalLength);
            childView.TranslationX = childState.TranslationX;
            childView.TranslationY = childState.TranslationY;
            childView.Scale = childState.Scale;
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        Clip = new RectangleGeometry(new Rect(0, 0, Width, Height));

        InvalidateMeasure();
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return new AdaptiveLayoutManager(this);
    }

    protected override void InvalidateMeasure()
    {
        base.InvalidateMeasure();
        (this as IView).InvalidateMeasure();
    }

    private static void Invalidate_OnRelativeHorizontalLengthChanged(
        BindableObject bindable,
        object oldValue,
        object newValue
    )
    {
        if (bindable is not Element element || element.Parent is not AdaptiveLayout parentLayout)
        {
            return;
        }

        // Do not invalidate if orientation is vertical and any of width factors changed
        if (parentLayout.Orientation == StackOrientation.Vertical)
        {
            return;
        }

        parentLayout.InvalidateMeasure();
    }

    private static void Invalidate_OnRelativeVerticalLengthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not Element element || element.Parent is not AdaptiveLayout parentLayout)
        {
            return;
        }

        // Do not invalidate if orientation is not vertical and any of height factors changed
        if (parentLayout.Orientation != StackOrientation.Vertical)
        {
            return;
        }

        parentLayout.InvalidateMeasure();
    }

    private static void Invalidate_OnOrientationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AdaptiveLayout layout)
        {
            layout.InvalidateMeasure();
        }
    }
}

public class AdaptiveLayoutManager(AdaptiveLayout layout) : ILayoutManager
{
    private readonly List<Size> _chidrenMeasurements = [];

    public List<Rect> CalculateChildrenSizes(
        Rect bounds,
        List<double?> relativeVerticalLengths,
        List<double?> relativeHorizontalLengths
    )
    {
        var result = new List<Rect>();

        if (layout.Orientation == StackOrientation.Vertical)
        {
            double y = bounds.Y;

            var childrenHeights = GetChildrenAbsoluteLengths(bounds.Height, relativeVerticalLengths);

            for (int i = 0; i < layout.Count; i++)
            {
                if (layout[i].Visibility == Visibility.Collapsed)
                {
                    result.Add(Rect.Zero);
                    continue;
                }

                var width = Math.Min(_chidrenMeasurements[i].Width, bounds.Width);
                var x = (bounds.Width - width) / 2;
                result.Add(new Rect(x, y, width, childrenHeights[i]));

                y += childrenHeights[i];
            }
        }
        else
        {
            double x = bounds.X;

            var childrenWidths = GetChildrenAbsoluteLengths(bounds.Width, relativeHorizontalLengths);

            for (int i = 0; i < layout.Count; i++)
            {
                if (layout[i].Visibility == Visibility.Collapsed)
                {
                    result.Add(Rect.Zero);
                    continue;
                }

                var height = Math.Min(_chidrenMeasurements[i].Height, bounds.Height);
                var y = (bounds.Height - height) / 2;
                result.Add(new Rect(x, y, childrenWidths[i], height));

                x += childrenWidths[i];
            }
        }

        return result;
    }

    public void MeasureChildren(
        Size constraints,
        List<double?> relativeVerticalLengths,
        List<double?> relativeHorizontalLengths
    )
    {
        _chidrenMeasurements.Clear();

        if (layout.Orientation == StackOrientation.Vertical)
        {
            var childrenHeights = GetChildrenAbsoluteLengths(constraints.Height, relativeVerticalLengths);

            for (int i = 0; i < layout.Count; i++)
            {
                var childSize = layout[i].Measure(constraints.Width, childrenHeights[i]);

                _chidrenMeasurements.Add(childSize);
            }
        }
        else
        {
            var childrenWidths = GetChildrenAbsoluteLengths(constraints.Width, relativeHorizontalLengths);

            for (int i = 0; i < layout.Count; i++)
            {
                var childSize = layout[i].Measure(childrenWidths[i], constraints.Height);

                _chidrenMeasurements.Add(childSize);
            }
        }
    }

    public Size ArrangeChildren(Rect bounds)
    {
        var relVerticalLengths = layout
            .Select(child => AdaptiveLayout.GetRelativeVerticalLength((BindableObject)child))
            .ToList();

        var relHorizontalLengths = layout
            .Select(child => AdaptiveLayout.GetRelativeHorizontalLength((BindableObject)child))
            .ToList();

        var boundsList = CalculateChildrenSizes(bounds, relVerticalLengths, relHorizontalLengths);

        for (int i = 0; i < layout.Count; i++)
        {
            if (layout[i].Visibility == Visibility.Collapsed)
            {
                continue;
            }

            layout[i].Arrange(boundsList[i]);
        }

        return bounds.Size.AdjustForFill(bounds, layout);
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        double width = 0;
        double height = 0;

        _chidrenMeasurements.Clear();

        if (layout.Orientation == StackOrientation.Vertical)
        {
            var childrenHeights = GetChildrenHeights(heightConstraint);

            for (int i = 0; i < layout.Count; i++)
            {
                var childSize = layout[i].Measure(widthConstraint, childrenHeights[i]);

                _chidrenMeasurements.Add(childSize);
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

                _chidrenMeasurements.Add(childSize);
                width += childSize.Width;
                height = Math.Max(height, childSize.Height);
            }
        }

        return new Size(width, height);
    }

    private List<double> GetChildrenHeights(double totalHeight)
    {
        var relativeLengths = layout
            .Select(child => AdaptiveLayout.GetRelativeVerticalLength((BindableObject)child))
            .ToList();
        return GetChildrenAbsoluteLengths(totalHeight, relativeLengths);
    }

    private List<double> GetChildrenWidths(double totalWidth)
    {
        var relativeLengths = layout
            .Select(child => AdaptiveLayout.GetRelativeHorizontalLength((BindableObject)child))
            .ToList();
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
            return relativeLengths
                .Select(relativeLength =>
                {
                    var factor = relativeLength ?? 0;
                    return factor * totalAbsoluteLength;
                })
                .ToList();
        }
        else
        {
            return relativeLengths
                .Select(relativeLength =>
                {
                    if (relativeLength.HasValue)
                    {
                        return relativeLength.Value * totalAbsoluteLength;
                    }
                    else
                    {
                        return 1 / nullRelativeLengthsCount * availableLength;
                    }
                })
                .ToList();
        }
    }
}
