namespace SubtitlesApp.Layouts;

public class AdaptiveLayoutState(List<ChildState> childrenStates, Rect bounds)
{
    public Rect Bounds { get; set; } = bounds;

    public List<ChildState> ChildrenStates { get; } = childrenStates;
}

public class ChildState
{
    public required double HorizontalLength { get; init; }

    public required double VerticalLength { get; init; }

    public double TranslationX { get; init; } = 0;

    public double TranslationY { get; init; } = 0;

    public double Scale { get; init; } = 1;

    public double? X { get; set; }

    public double? Y { get; set; }

    public double? Width { get; init; }

    public double? Height { get; init; }

    public Rect GetBounds()
    {
        if (X is null || Y is null || Width is null || Height is null)
        {
            return Rect.Zero;
        }

        return new Rect(X.Value, Y.Value, Width.Value, Height.Value);
    }
}
