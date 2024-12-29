using MauiPageFullScreen;
using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class MediaElementPage : ContentPage
{
    public MediaElementPage(MediaElementViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        var vm = (MediaElementViewModel)BindingContext;
        vm.Clean();
        mediaPlayer.Stop();
        mediaPlayer.DisconnectHandler();

        base.OnNavigatedFrom(args);
    }

    protected override bool OnBackButtonPressed()
    {
        var vm = (MediaElementViewModel)BindingContext;

        // On back button press just show subtitles if they are hidden
        // Otherwise, proceed with exit
        if (!vm.IsSideChildVisible)
        {
            vm.IsSideChildVisible = true;
            return true;
        }

        if (Controls.IsFullScreen)
        {
            Controls.RestoreScreen();
        }

        return false;
    }
}