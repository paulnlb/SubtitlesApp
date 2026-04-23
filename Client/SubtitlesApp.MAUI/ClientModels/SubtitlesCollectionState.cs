using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitlesApp.ClientModels;

public partial class SubtitlesCollectionState : ObservableObject
{
    [ObservableProperty]
    private int _currentSubtitleIndex;

    [ObservableProperty]
    private int _firstVisibleSubtitleIndex;

    [ObservableProperty]
    private int _lastVisibleSubtitleIndex;

    [ObservableProperty]
    private bool _autoScrollEnabled;

    partial void OnFirstVisibleSubtitleIndexChanged(int value)
    {
        if (value - CurrentSubtitleIndex >= 2)
        {
            AutoScrollEnabled = false;
        }
        else
        {
            AutoScrollEnabled = true;
        }
    }

    partial void OnLastVisibleSubtitleIndexChanged(int value)
    {
        if (CurrentSubtitleIndex - value >= 2)
        {
            AutoScrollEnabled = false;
        }
        else
        {
            AutoScrollEnabled = true;
        }
    }
}
