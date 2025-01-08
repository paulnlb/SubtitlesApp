using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitlesApp.ClientModels;

public partial class PlayerSubtitlesLayoutSettings : ObservableObject
{
    [ObservableProperty]
    bool _isSideChildVisible = true;

    [ObservableProperty]
    double _playerRelativeVerticalLength;

    [ObservableProperty]
    double _playerRelativeHorizontalLength;

    public delegate void IsSideChildVisibleChangedEventHandler(bool newValue);

    public event IsSideChildVisibleChangedEventHandler IsSideChildVisibleChanged;

    partial void OnIsSideChildVisibleChanged(bool value)
    {
        IsSideChildVisibleChanged.Invoke(value);
    }
}
