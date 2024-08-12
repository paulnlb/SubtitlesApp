using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SubtitlesApp.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    public MainPageViewModel()
    {
    }

    [RelayCommand]
    public void OpenSettings() => Shell.Current.GoToAsync($"settings");
}
