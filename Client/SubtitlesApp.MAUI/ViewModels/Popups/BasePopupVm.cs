using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SubtitlesApp.ViewModels.Popups;

public abstract partial class BasePopupVm : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _acceptText = "Save";

    [ObservableProperty]
    private string _cancelText = "Cancel";

    [ObservableProperty]
    private bool _isAcceptEnabled = true;

    [ObservableProperty]
    private bool _isCancelVisible = true;

    [RelayCommand]
    public abstract Task Accept();

    [RelayCommand]
    public abstract Task Cancel();
}
