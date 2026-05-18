namespace SubtitlesApp.Interfaces.Settings;

public interface ILayoutSettings
{
    double PlayerVerticalLength { get; set; }

    double SubtitlesVerticalLength { get; set; }

    double PlayerHorizontalLength { get; set; }

    double SubtitlesHoritzontalLength { get; set; }

    double MaxPlayerRelativeVerticalLength { get; }

    double PlaceholderVerticalLength { get; set; }

    double PlaceholderHorizontalLength { get; set; }
}
