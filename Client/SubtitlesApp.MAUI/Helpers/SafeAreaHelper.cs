namespace SubtitlesApp.Helpers;

public static class SafeAreaHelper
{
    public static void DisableSafeAreas(Element root)
    {
        Apply(root);
    }

    private static void Apply(Element element)
    {
        switch (element)
        {
            case ContentPage page:
                page.SafeAreaEdges = SafeAreaEdges.None;
                break;

            case Layout layout:
                layout.SafeAreaEdges = SafeAreaEdges.None;
                break;

            case ScrollView scrollView:
                scrollView.SafeAreaEdges = SafeAreaEdges.None;
                break;

            case ContentView contentView:
                contentView.SafeAreaEdges = SafeAreaEdges.None;
                break;

            case Border border:
                border.SafeAreaEdges = SafeAreaEdges.None;
                break;
        }

        if (element is IElementController controller)
        {
            foreach (var child in controller.LogicalChildren)
            {
                Apply(child);
            }
        }
    }
}
