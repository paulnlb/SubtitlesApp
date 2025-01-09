using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitlesApp.ClientModels;

public partial class SubtitlesCollectionState : ObservableObject
{
    [ObservableProperty]
    int _currentSubtitleIndex;

    [ObservableProperty]
    int _scrollToSubtitleIndex;

    [ObservableProperty]
    int _firstVisibleSubtitleIndex;

    [ObservableProperty]
    int _lastVisibleSubtitleIndex;

    [ObservableProperty]
    bool _autoScrollEnabled;
}
