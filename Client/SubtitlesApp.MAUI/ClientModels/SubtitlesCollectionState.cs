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

    [ObservableProperty]
    bool _isTranslationRunning;

    public delegate void AutoScrollEnabledChangedEventHandler(bool newValue);

    public event AutoScrollEnabledChangedEventHandler AutoScrollEnabledChanged;

    partial void OnAutoScrollEnabledChanged(bool value)
    {
        AutoScrollEnabledChanged?.Invoke(value);
    }
}
