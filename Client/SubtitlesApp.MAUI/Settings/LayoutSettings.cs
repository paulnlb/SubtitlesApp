using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitlesApp.Settings;

public partial class LayoutSettings : ObservableObject
{
    [ObservableProperty]
    private double _playerVerticalLength;

    [ObservableProperty]
    private double _subtitlesVerticalLength;

    [ObservableProperty]
    private double _playerHorizontalLength;

    [ObservableProperty]
    private double _subtitlesHoritzontalLength;
    public double MaxPlayerRelativeVerticalLength => 0.5;

    public LayoutSettings(bool isExpanded)
    {
        if (isExpanded)
        {
            _playerVerticalLength = _playerHorizontalLength = 1;
        }
        else
        {
            _playerVerticalLength = 0.3;
            _playerHorizontalLength = 0.6;
        }

        _subtitlesVerticalLength = 0.6;
        _subtitlesHoritzontalLength = 0.4;
    }

    public void CopyFrom(LayoutSettings settinsToCopy)
    {
        PlayerVerticalLength = settinsToCopy.PlayerVerticalLength;
        PlayerHorizontalLength = settinsToCopy.PlayerHorizontalLength;
        SubtitlesVerticalLength = settinsToCopy.SubtitlesVerticalLength;
        SubtitlesHoritzontalLength = settinsToCopy.SubtitlesHoritzontalLength;
    }
}
