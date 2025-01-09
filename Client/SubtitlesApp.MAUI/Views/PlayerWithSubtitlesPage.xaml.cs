using MauiPageFullScreen;
using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class PlayerWithSubtitlesPage : ContentPage
{
    public PlayerWithSubtitlesPage(PlayerWithSubtitlesViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        var vm = (PlayerWithSubtitlesViewModel)BindingContext;
        vm.Clean();
        mediaPlayer.Stop();
        mediaPlayer.DisconnectHandler();

        base.OnNavigatedFrom(args);
    }

    protected override bool OnBackButtonPressed()
    {
        var vm = (PlayerWithSubtitlesViewModel)BindingContext;

        // On back button press just show subtitles if they are hidden
        // Otherwise, proceed with exit
        if (!vm.LayoutSettings.IsSideChildVisible)
        {
            vm.LayoutSettings.IsSideChildVisible = true;
            return true;
        }

        if (Controls.IsFullScreen)
        {
            Controls.RestoreScreen();
        }

        return false;
    }
}
