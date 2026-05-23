namespace SubtitlesApp.Layouts;

public class PlayerSubtitlesStateManager
{
    private readonly AdaptiveLayout layout;

    private const int Capacity = 2;

    public PlayerSubtitlesStateManager(AdaptiveLayout layout)
    {
        LayoutStates = [];
        this.layout = layout;
    }

    public List<AdaptiveLayoutState> LayoutStates { get; private set; }

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

    public AdaptiveLayoutState PreCalcState()
    {
        var manager = new AdaptiveLayoutManager(layout);

        var childrenSizes = manager.CalculateChildrenSizes(new Rect(0, 0, layout.Width, layout.Height));

        var mediaPlayerView =
            layout.Children[1] as BindableObject
            ?? throw new InvalidOperationException("Player view is not a BindableObject.");

        var subtitlesView =
            layout.Children[1] as BindableObject
            ?? throw new InvalidOperationException("Subtitles view is not a BindableObject.");

        var childrenStates = new List<ChildState>()
        {
            new()
            {
                HorizontalLength = AdaptiveLayout.GetRelativeHorizontalLength(mediaPlayerView) ?? 0,
                VerticalLength = AdaptiveLayout.GetRelativeVerticalLength(mediaPlayerView) ?? 0,
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
                HorizontalLength = AdaptiveLayout.GetRelativeHorizontalLength(subtitlesView) ?? 0,
                VerticalLength = AdaptiveLayout.GetRelativeVerticalLength(subtitlesView) ?? 0,
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
