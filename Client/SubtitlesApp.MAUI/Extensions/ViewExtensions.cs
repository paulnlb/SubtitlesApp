using SubtitlesApp.Helpers;

namespace SubtitlesApp.Extensions;

public static class ViewExtensions
{
    public static void Transform(this View view, Transformation transformation)
    {
        view.Scale = transformation.Scale;
        view.TranslationX = transformation.TranslateX;
        view.TranslationY = transformation.TranslateY;
    }

    public static void ResetTransformations(this View view)
    {
        view.Scale = 1;
        view.TranslationX = 0;
        view.TranslationY = 0;
    }
}
