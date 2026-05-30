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
    private double _playerRelativeVerticalLength;

    [ObservableProperty]
    private double _playerRelativeHorizontalLength;

    [ObservableProperty]
    private bool _isImmersiveOn;

    [ObservableProperty]
    private bool _isFullScreenOn;

    [ObservableProperty]
    CaptionsViewModel _captionsVm;

    #endregion

    public PlayerWithSubtitlesViewModel(CaptionsViewModel captionsViewModel)
    {
        PlayerControlsVisible = true;
        MediaPath = null;
        CaptionsVm = captionsViewModel;
    }

    [RelayCommand]
    public void TogglePlayerControlsVisibility()
    {
        PlayerControlsVisible = !PlayerControlsVisible;
    }

    #region private methods
    void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("open", out object? value))
        {
            MediaPath = value.ToString();
            CaptionsVm.MediaPath = value.ToString();
        }
    }

    #endregion
}
