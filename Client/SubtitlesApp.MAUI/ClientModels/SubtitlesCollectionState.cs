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

    [ObservableProperty]
    private bool _isTranslationRunning;

    public delegate void AutoScrollEnabledChangedEventHandler(bool newValue);

    public event AutoScrollEnabledChangedEventHandler AutoScrollEnabledChanged;

    partial void OnAutoScrollEnabledChanged(bool value)
    {
        AutoScrollEnabledChanged?.Invoke(value);
    }
}
