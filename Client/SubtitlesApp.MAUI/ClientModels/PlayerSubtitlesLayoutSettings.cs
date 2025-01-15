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
    private SwipeDirection _showSubtitlesSwipeDirection;

    [ObservableProperty]
    private SwipeDirection _hideSubtitlesSwipeDirection;

    public delegate void IsSideChildVisibleChangedEventHandler(bool newValue);

    public event IsSideChildVisibleChangedEventHandler IsSideChildVisibleChanged;

    partial void OnIsSideChildVisibleChanged(bool value)
    {
        IsSideChildVisibleChanged?.Invoke(value);
    }
}
