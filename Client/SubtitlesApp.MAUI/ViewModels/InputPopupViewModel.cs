using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SubtitlesApp.ViewModels;

public partial class InputPopupViewModel(IPopupService popupService) : ObservableObject
{
    [ObservableProperty]
    string _url;

    [RelayCommand]
    public Task Ok()
    {
        return popupService.ClosePopupAsync(Url);
    }

    [RelayCommand]
    public Task Cancel()
    {
        return popupService.ClosePopupAsync();
    }
}
