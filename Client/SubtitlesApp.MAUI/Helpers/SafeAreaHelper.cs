namespace SubtitlesApp.Helpers;

public static class SafeAreaHelper
{
    public static void DisableSafeAreas(Element root)
    {
        Apply(root, SafeAreaEdges.None);
    }

    public static void ResetSafeAreas(Element root)
    {
        RestoreDefaults(root);
    }

    public static void ContainerizeSafeAreas(Element root)
    {
        Apply(root, new SafeAreaEdges(SafeAreaRegions.Container));
    }

    private static void Apply(Element element, SafeAreaEdges safeAreaEdges)
    {
        switch (element)
        {
            case ContentPage page:
                page.SafeAreaEdges = safeAreaEdges;
                break;

            case Layout layout:
                layout.SafeAreaEdges = safeAreaEdges;
                break;

            case ScrollView scrollView:
                scrollView.SafeAreaEdges = safeAreaEdges;
                break;

            case ContentView contentView:
                contentView.SafeAreaEdges = safeAreaEdges;
                break;

            case Border border:
                border.SafeAreaEdges = safeAreaEdges;
                break;
        }

        if (element is IElementController controller)
        {
            foreach (var child in controller.LogicalChildren)
            {
                Apply(child, safeAreaEdges);
            }
        }
    }

    private static void RestoreDefaults(Element element)
    {
        switch (element)
        {
            case ContentPage page:
                page.SafeAreaEdges = SafeAreaEdges.None;
                break;

            case Layout layout:
                layout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
                break;

            case ScrollView scrollView:
                scrollView.SafeAreaEdges = SafeAreaEdges.Default;
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
                RestoreDefaults(child);
            }
        }
    }
}
