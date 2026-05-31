using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.Layouts;

/// <summary>
///     Custom layout that arranges views in one column (in portrait mode) or row (otherwise).
///     <para/>
///     Layout orientation can be set manually or calculated dynamically at runtime, based on the parent's aspect ratio.
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

    public static readonly BindableProperty OrientationRequestProperty = BindableProperty.Create(
        nameof(OrientationRequest),
        typeof(AdaptiveLayoutOrientation),
        typeof(AdaptiveLayout),
        AdaptiveLayoutOrientation.Adaptive,
        propertyChanged: Invalidate_OnOrientationRequestChanged
    );

    public static readonly BindableProperty ActualOrientationProperty = BindableProperty.Create(
        nameof(ActualOrientation),
        typeof(AdaptiveLayoutOrientation),
        typeof(AdaptiveLayout),
        AdaptiveLayoutOrientation.Adaptive,
        propertyChanged: OnActualOrientationChanged
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

    public AdaptiveLayoutOrientation OrientationRequest
    {
        get => (AdaptiveLayoutOrientation)GetValue(OrientationRequestProperty);
        set => SetValue(OrientationRequestProperty, value);
    }

    public AdaptiveLayoutOrientation ActualOrientation
    {
        get
        {
            if (OrientationRequest != AdaptiveLayoutOrientation.Adaptive)
            {
                return OrientationRequest;
            }

            if (Width > Height)
            {
                return AdaptiveLayoutOrientation.Horizontal;
            }
            else
            {
                return AdaptiveLayoutOrientation.Vertical;
            }
        }
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

        OnPropertyChanged(nameof(ActualOrientation));
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
        if (parentLayout.ActualOrientation == AdaptiveLayoutOrientation.Vertical)
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

        // Do not invalidate if orientation is horizontal and any of height factors changed
        if (parentLayout.ActualOrientation == AdaptiveLayoutOrientation.Horizontal)
        {
            return;
        }

        parentLayout.InvalidateMeasure();
    }

    private static void Invalidate_OnOrientationRequestChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AdaptiveLayout layout)
        {
            layout.InvalidateMeasure();
        }
    }

    private static void OnActualOrientationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((AdaptiveLayout)bindable).OnPropertyChanged(nameof(ActualOrientation));
    }
}

public class AdaptiveLayoutManager(AdaptiveLayout layout) : ILayoutManager
{
    private readonly List<Size> _chidrenMeasurements = [];
    private AdaptiveLayoutOrientation _layoutOrientation;

    public Size ArrangeChildren(Rect bounds)
    {
        var relVerticalLengths = layout.Select(child => AdaptiveLayout.GetRelativeVerticalLength((BindableObject)child));
        var relHorizontalLengths = layout.Select(child => AdaptiveLayout.GetRelativeHorizontalLength((BindableObject)child));

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

        var relVerticalLengths = layout.Select(child => AdaptiveLayout.GetRelativeVerticalLength((BindableObject)child));
        var relHorizontalLengths = layout.Select(child => AdaptiveLayout.GetRelativeHorizontalLength((BindableObject)child));

        MeasureChildren(widthConstraint, heightConstraint, relVerticalLengths, relHorizontalLengths);

        if (_layoutOrientation == AdaptiveLayoutOrientation.Vertical)
        {
            for (int i = 0; i < layout.Count; i++)
            {
                var childSize = _chidrenMeasurements[i];

                width = Math.Max(width, childSize.Width);
                height += childSize.Height;
            }
        }
        else if (_layoutOrientation == AdaptiveLayoutOrientation.Horizontal)
        {
            for (int i = 0; i < layout.Count; i++)
            {
                var childSize = _chidrenMeasurements[i];

                width += childSize.Width;
                height = Math.Max(height, childSize.Height);
            }
        }
        else
        {
            throw new InvalidOperationException($"Unsupported layout orientation: {_layoutOrientation}");
        }

        return new Size(width, height);
    }

    public AdaptiveLayoutState ComputeState(List<double?> relHeights, List<double?> relWidths)
    {
        var bounds = new Rect(0, 0, layout.Width, layout.Height);

        MeasureChildren(layout.Width, layout.Height, relHeights, relWidths);
        var childrenSizes = CalculateChildrenSizes(bounds, relHeights, relWidths);

        var childrenStates = new List<ChildState>();

        for (int i = 0; i < childrenSizes.Count; i++)
        {
            childrenStates.Add(
                new()
                {
                    HorizontalLength = relWidths[i]!.Value,
                    VerticalLength = relHeights[i]!.Value,
                    TranslationX = 0,
                    TranslationY = 0,
                    Scale = 1,
                    X = childrenSizes[i].X,
                    Y = childrenSizes[i].Y,
                    Width = childrenSizes[i].Width,
                    Height = childrenSizes[i].Height,
                }
            );
        }

        return new AdaptiveLayoutState(childrenStates, layout.Bounds);
    }

    private List<Rect> CalculateChildrenSizes(
        Rect bounds,
        IEnumerable<double?> relativeVerticalLengths,
        IEnumerable<double?> relativeHorizontalLengths
    )
    {
        var result = new List<Rect>();

        if (_layoutOrientation == AdaptiveLayoutOrientation.Vertical)
        {
            double y = bounds.Y;

            var childrenHeights = GetChildrenAbsoluteLengths(bounds.Height, relativeVerticalLengths.ToList());

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
        else if (_layoutOrientation == AdaptiveLayoutOrientation.Horizontal)
        {
            double x = bounds.X;

            var childrenWidths = GetChildrenAbsoluteLengths(bounds.Width, relativeHorizontalLengths.ToList());

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
        else
        {
            throw new InvalidOperationException($"Unsupported layout orientation: {_layoutOrientation}");
        }

        return result;
    }

    private void MeasureChildren(
        double widthConstraint,
        double heightConstraint,
        IEnumerable<double?> relativeVerticalLengths,
        IEnumerable<double?> relativeHorizontalLengths
    )
    {
        _chidrenMeasurements.Clear();
        _layoutOrientation = layout.ActualOrientation;

        if (_layoutOrientation == AdaptiveLayoutOrientation.Vertical)
        {
            var childrenHeights = GetChildrenAbsoluteLengths(heightConstraint, relativeVerticalLengths.ToList());

            for (int i = 0; i < layout.Count; i++)
            {
                var childSize = layout[i].Measure(widthConstraint, childrenHeights[i]);

                _chidrenMeasurements.Add(childSize);
            }
        }
        else
        {
            var childrenWidths = GetChildrenAbsoluteLengths(widthConstraint, relativeHorizontalLengths.ToList());

            for (int i = 0; i < layout.Count; i++)
            {
                var childSize = layout[i].Measure(childrenWidths[i], heightConstraint);

                _chidrenMeasurements.Add(childSize);
            }
        }
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
