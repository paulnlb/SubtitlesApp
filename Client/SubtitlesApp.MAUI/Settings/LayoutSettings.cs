using SubtitlesApp.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class LayoutSettings : ILayoutSettings
{
    public double PlayerVerticalLength { get; set; } = 0.3;
    public double SubtitlesVerticalLength { get; set; } = 0.7;
    public double PlayerHorizontalLength { get; set; } = 0.6;
    public double SubtitlesHoritzontalLength { get; set; } = 0.4;
    public double MaxPlayerRelativeVerticalLength => 0.5;
    public double StatusBarVerticalLength { get; set; }
    public double StatusBarHorizontalLength { get; set; }
    public double NavBarVerticalLength { get; set; }
    public double NavBarHorizontalLength { get; set; }
}
