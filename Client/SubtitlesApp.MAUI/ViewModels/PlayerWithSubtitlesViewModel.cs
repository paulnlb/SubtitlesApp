using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SubtitlesApp.ViewModels;

public partial class PlayerWithSubtitlesViewModel : ObservableObject, IQueryAttributable
{
    #region observable properties

    [ObservableProperty]
    private string? _mediaPath;

    [ObservableProperty]
    private bool _playerControlsVisible;

    [ObservableProperty]
    private bool _isImmersiveOn;

    [ObservableProperty]
    private bool _isFullScreenOn;

    [ObservableProperty]
    SubtitlesViewModel _subtitlesVm;

    #endregion

    public PlayerWithSubtitlesViewModel(SubtitlesViewModel captionsViewModel)
    {
        PlayerControlsVisible = true;
        MediaPath = null;
        SubtitlesVm = captionsViewModel;
    }

    [RelayCommand]
    public void TogglePlayerControlsVisibility()
    {
        PlayerControlsVisible = !PlayerControlsVisible;
    }

    partial void OnIsFullScreenOnChanged(bool value)
    {
        IsImmersiveOn = value;
        PlayerControlsVisible = false;
    }

    void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("open", out object? value))
        {
            MediaPath = value.ToString();
            SubtitlesVm.MediaPath = value.ToString();
        }
    }
}
