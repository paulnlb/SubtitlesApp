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
}
