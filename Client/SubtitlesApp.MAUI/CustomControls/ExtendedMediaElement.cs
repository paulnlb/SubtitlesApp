using CommunityToolkit.Maui.Views;

namespace SubtitlesApp.CustomControls;

public class ExtendedMediaElement : MediaElement
{
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        if (MediaHeight == 0 || MediaWidth == 0)
        {
            return base.MeasureOverride(widthConstraint, heightConstraint);
        }

        var adjustedSize = GetMediaSize(MediaWidth, MediaHeight, widthConstraint, heightConstraint);

        if (Handler == null)
        {
            return Size.Zero;
        }

        var margin = Margin;

        // Adjust the constraints to account for the margins
        widthConstraint -= margin.HorizontalThickness;
        heightConstraint -= margin.VerticalThickness;

        var desiredWidth = Math.Min(widthConstraint, adjustedSize.Width);
        var desiredHeight = Math.Min(heightConstraint, adjustedSize.Height);

        // Ask the handler to do the actual measuring
        var measureWithoutMargins = Handler.GetDesiredSize(desiredWidth, desiredHeight);

        // Account for the margins when reporting the desired size value
        return new Size(
            measureWithoutMargins.Width + margin.HorizontalThickness,
            measureWithoutMargins.Height + margin.VerticalThickness
        );
    }

    private static Size GetMediaSize(double mediaWidth, double mediaHeight, double widthConstraint, double heightConstraint)
    {
        if (mediaWidth == 0 || mediaHeight == 0)
        {
            return Size.Zero;
        }

        var mediaAspectRatio = (double)mediaWidth / mediaHeight;
        var controlAspectRatio = widthConstraint / heightConstraint;

        double displayedMediaWidth,
            displayedMediaHeight;

        if (mediaAspectRatio > controlAspectRatio)
        {
            // Media is wider than the control, so it will be letterboxed on the top and bottom
            displayedMediaWidth = widthConstraint;
            displayedMediaHeight = widthConstraint / mediaAspectRatio;
        }
        else
        {
            // Media is taller than the control, so it will be letterboxed on the left and right
            displayedMediaHeight = heightConstraint;
            displayedMediaWidth = heightConstraint * mediaAspectRatio;
        }

        return new Size(displayedMediaWidth, displayedMediaHeight);
    }
}
