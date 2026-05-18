using SubtitlesApp.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class LayoutSettings : ILayoutSettings
{
    public double PlayerVerticalLength { get; set; } = 0.3;
    public double SubtitlesVerticalLength { get; set; } = 0.7;
    public double PlayerHorizontalLength { get; set; } = 0.65;
    public double SubtitlesHoritzontalLength { get; set; } = 0.35;
    public double MaxPlayerRelativeVerticalLength => 0.5;
    public double PlaceholderVerticalLength { get; set; }
    public double PlaceholderHorizontalLength { get; set; }
}
