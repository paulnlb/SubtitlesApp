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
    public double MaxPlayerVerticalLength => 0.5;
    public double MinPlayerVerticalLength => 0.25;

    public LayoutSettings(bool isExpanded)
    {
        if (isExpanded)
        {
            _playerVerticalLength = _playerHorizontalLength = 1;
        }
        else
        {
            _playerVerticalLength = 0.35;
            _playerHorizontalLength = 0.65;
        }

        _subtitlesVerticalLength = 0.65;
        _subtitlesHoritzontalLength = 0.35;
    }

    public void CopyFrom(LayoutSettings settinsToCopy)
    {
        PlayerVerticalLength = settinsToCopy.PlayerVerticalLength;
        PlayerHorizontalLength = settinsToCopy.PlayerHorizontalLength;
        SubtitlesVerticalLength = settinsToCopy.SubtitlesVerticalLength;
        SubtitlesHoritzontalLength = settinsToCopy.SubtitlesHoritzontalLength;
    }
}
