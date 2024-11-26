using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private bool _isLoggedOut;

    public MainPageViewModel(IAuthService authService)
    {
        _authService = authService;

        _isLoggedIn = _authService.GetAccesTokenAsync().Result is not null;
        _isLoggedOut = !_isLoggedIn;
    }

    [RelayCommand]
    public async Task LogInAsync()
    {
        var result = await _authService.LogInAsync();
        if (result.IsSuccess)
        {
            IsLoggedIn = true;
            IsLoggedOut = false;
        }
    }

    [RelayCommand]
    public async Task LogOutAsync()
    {
        var result = await _authService.LogOutAsync();
        if (result.IsSuccess)
        {
            IsLoggedIn = false;
            IsLoggedOut = true;
        }
    }

    [RelayCommand]
    public void OpenSettings() => Shell.Current.GoToAsync($"settings");
}
