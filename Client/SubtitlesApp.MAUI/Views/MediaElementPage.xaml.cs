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

        if (!vm.IsSideChildVisible)
        {
            vm.IsSideChildVisible = true;

            if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait) 
            {
                Controls.RestoreScreen();
            }
            return true;
        }

        Dispatcher.Dispatch(async () =>
        {
            var userWantsToExit = await DisplayAlert(
                "Achtung",
                "Are you sure you want to exit player?",
                "Yes",
                "Cancel");

            if (userWantsToExit)
            {
                await Shell.Current.Navigation.PopAsync();
            }
        });

        return true;
    }
}