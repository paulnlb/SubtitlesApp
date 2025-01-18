using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitlesApp.ClientModels;

public partial class PlayerSubtitlesLayoutSettings : ObservableObject
{
    [ObservableProperty]
    private bool _isSideChildVisible;

    [ObservableProperty]
    private double _playerRelativeVerticalLength;

    [ObservableProperty]
    private double _playerRelativeHorizontalLength;

    [ObservableProperty]
    private double _subtitlesRelativeVerticalLength;

    [ObservableProperty]
    private double _subtitlesRelativeHorizontalLength;

    [ObservableProperty]
    private SwipeDirection _showSubtitlesSwipeDirection;

    [ObservableProperty]
    private SwipeDirection _hideSubtitlesSwipeDirection;

    [ObservableProperty]
    private int _videoWidthPx;

    [ObservableProperty]
    private int _videoHeightPx;

    [ObservableProperty]
    private double _newPlayerRelativeVerticalLength = -1;

    [ObservableProperty]
    private double _newPlayerRelativeHorizontalLength = -1;

    public delegate void IsSideChildVisibleChangedEventHandler(bool newValue);

    public event IsSideChildVisibleChangedEventHandler IsSideChildVisibleChanged;

    partial void OnIsSideChildVisibleChanged(bool value)
    {
        IsSideChildVisibleChanged?.Invoke(value);
    }

    partial void OnVideoHeightPxChanged(int value)
    {
        RecalculateRelativePlayerHeight();
    }

    partial void OnVideoWidthPxChanged(int value)
    {
        RecalculateRelativePlayerHeight();
    }

    private void RecalculateRelativePlayerHeight()
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
}
