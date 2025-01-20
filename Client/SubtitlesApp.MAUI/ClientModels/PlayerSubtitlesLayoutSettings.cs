using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitlesApp.ClientModels;

public partial class PlayerSubtitlesLayoutSettings : ObservableObject
{
    private const double MaxPlayerRelativeVerticalLength = 0.5;

    [ObservableProperty]
    private double _playerRelativeVerticalLength;

    [ObservableProperty]
    private double _playerRelativeHorizontalLength;

    [ObservableProperty]
    private double _subtitlesRelativeVerticalLength;

    [ObservableProperty]
    private double _subtitlesRelativeHorizontalLength;

    [ObservableProperty]
    private int _videoWidthPx;

    [ObservableProperty]
    private int _videoHeightPx;

    public void RecalculateVerticalLayout()
    {
        if (DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
        {
            return;
        }

        var newRelativeHeight =
            (VideoHeightPx * Shell.Current.CurrentPage.Width) / (Shell.Current.CurrentPage.Height * VideoWidthPx);

        if (newRelativeHeight == 0 || double.IsNaN(newRelativeHeight))
        {
            return;
        }

        newRelativeHeight = Math.Min(MaxPlayerRelativeVerticalLength, newRelativeHeight);

        PlayerRelativeVerticalLength = newRelativeHeight;
        SubtitlesRelativeVerticalLength = 1 - newRelativeHeight;
    }

    partial void OnVideoHeightPxChanged(int value)
    {
        RecalculateVerticalLayout();
    }

    partial void OnVideoWidthPxChanged(int value)
    {
        RecalculateVerticalLayout();
    }
}
