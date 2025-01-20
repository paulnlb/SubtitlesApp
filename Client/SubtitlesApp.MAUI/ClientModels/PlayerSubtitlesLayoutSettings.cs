using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitlesApp.ClientModels;

public partial class PlayerSubtitlesLayoutSettings : ObservableObject
{
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

    public delegate void LayoutRecalculatedEventHandler();

    public event LayoutRecalculatedEventHandler LayoutRecalculated;

    public void RecalculateLayout()
    {
        var newRelativeHeight =
            (VideoHeightPx * Shell.Current.CurrentPage.Width) / (Shell.Current.CurrentPage.Height * VideoWidthPx);

        if (newRelativeHeight == 0 || double.IsNaN(newRelativeHeight))
        {
            return;
        }

        newRelativeHeight = Math.Min(0.5, newRelativeHeight);

        PlayerRelativeVerticalLength = newRelativeHeight;
        SubtitlesRelativeVerticalLength = 1 - newRelativeHeight;
    }

    partial void OnVideoHeightPxChanged(int value)
    {
        RecalculateLayout();
    }

    partial void OnVideoWidthPxChanged(int value)
    {
        RecalculateLayout();
    }

    partial void OnPlayerRelativeVerticalLengthChanged(double value)
    {
        LayoutRecalculated?.Invoke();
    }

    partial void OnPlayerRelativeHorizontalLengthChanged(double value)
    {
        LayoutRecalculated?.Invoke();
    }

    partial void OnSubtitlesRelativeHorizontalLengthChanged(double value)
    {
        LayoutRecalculated?.Invoke();
    }

    partial void OnSubtitlesRelativeVerticalLengthChanged(double value)
    {
        LayoutRecalculated?.Invoke();
    }
}
