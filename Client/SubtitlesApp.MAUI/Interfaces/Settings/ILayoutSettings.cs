namespace SubtitlesApp.Interfaces.Settings;

public interface ILayoutSettings
{
    double PlayerVerticalLength { get; set; }

    double SubtitlesVerticalLength { get; set; }

    double PlayerHorizontalLength { get; set; }

    double SubtitlesHoritzontalLength { get; set; }

    double MaxPlayerRelativeVerticalLength { get; }

    double StatusBarVerticalLength { get; set; }

    double StatusBarHorizontalLength { get; set; }

    double NavBarVerticalLength { get; set; }

    double NavBarHorizontalLength { get; set; }
}
