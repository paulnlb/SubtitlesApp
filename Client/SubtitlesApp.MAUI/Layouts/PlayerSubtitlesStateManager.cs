namespace SubtitlesApp.Layouts;

public class PlayerSubtitlesStateManager(AdaptiveLayout layout)
{
    private readonly AdaptiveLayout layout = layout;

    private const int Capacity = 2;

    public List<AdaptiveLayoutState> LayoutStates { get; private set; } = [];

    public void PushCurrentState()
    {
        AddSnapshot(layout.MakeSnapshot());
    }

    public void AddSnapshot(AdaptiveLayoutState snapshot)
    {
        LayoutStates.Add(snapshot);

        if (LayoutStates.Count > Capacity)
        {
            LayoutStates.RemoveAt(0);
        }
    }

    public void RestoreFrom(int index = 1)
    {
        layout.Restore(PeekSnapshot(index));
    }

    public AdaptiveLayoutState PeekSnapshot(int index = 1)
    {
        return LayoutStates[^index];
    }

    public void ClearSnapshots()
    {
        LayoutStates.Clear();
    }

    public AdaptiveLayoutState PreCalcState(List<double?> relativeVerticalLengths, List<double?> relativeHorizontalLengths)
    {
        var manager = new AdaptiveLayoutManager(layout);

        var bounds = new Rect(0, 0, layout.Width, layout.Height);
        var relHeights = relativeVerticalLengths;
        var relWidths = relativeHorizontalLengths;

        var childrenSizes = manager.CalculateChildrenSizes(bounds, relHeights, relWidths);

        var childrenStates = new List<ChildState>()
        {
            new()
            {
                HorizontalLength = relWidths[0]!.Value,
                VerticalLength = relHeights[0]!.Value,
                TranslationX = 0,
                TranslationY = 0,
                Scale = 1,
                X = childrenSizes[0].X,
                Y = childrenSizes[0].Y,
                Width = childrenSizes[0].Width,
                Height = childrenSizes[0].Height,
            },
            new()
            {
                HorizontalLength = relWidths[1]!.Value,
                VerticalLength = relHeights[1]!.Value,
                TranslationX = 0,
                TranslationY = 0,
                Scale = 1,
                X = childrenSizes[1].X,
                Y = childrenSizes[1].Y,
                Width = childrenSizes[1].Width,
                Height = childrenSizes[1].Height,
            },
        };

        return new AdaptiveLayoutState(childrenStates, layout.Bounds);
    }
}
