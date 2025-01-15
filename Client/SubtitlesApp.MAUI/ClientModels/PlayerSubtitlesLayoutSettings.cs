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

    public delegate void IsSideChildVisibleChangedEventHandler(bool newValue);

    public event IsSideChildVisibleChangedEventHandler IsSideChildVisibleChanged;

    partial void OnIsSideChildVisibleChanged(bool value)
    {
        IsSideChildVisibleChanged?.Invoke(value);
    }
}
