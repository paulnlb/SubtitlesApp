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

    protected override void OnDisappearing()
    {
        customLayout.Unsubscribe();

        base.OnDisappearing();
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
        if (!customLayout.IsSideChildVisible)
        {
            customLayout.IsSideChildVisible = true;

            if (customLayout.Orientation == StackOrientation.Vertical) 
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