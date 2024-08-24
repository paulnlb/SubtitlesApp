using MauiPageFullScreen;
using SubtitlesApp.CustomControls;
using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class MediaElementPage : ContentPage
{
    public MediaElementPage(MediaElementViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    async void OnSubtileTapped(object sender, SubtitleTappedEventArgs e)
    {
        var subtitle = e.Subtitle;
        await mediaPlayer.SeekTo(subtitle.TimeInterval.StartTime, CancellationToken.None);
    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        var vm = (MediaElementViewModel)BindingContext;
        await vm.CleanAsync();
        mediaPlayer.Stop();
        mediaPlayer.DisconnectHandler();
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

    void OnOrientationChanged(object sender, LayoutChangedEventArgs e)
    {
        if (e.Orientation == StackOrientation.Horizontal)
        {
            Controls.FullScreen();
        }
        else if (e.Orientation == StackOrientation.Vertical && customLayout.IsSideChildVisible)
        {
            Controls.RestoreScreen();
        }
    }

    void OnPlayerTapped(object sender, EventArgs e)
    {
        mediaPlayer.PlayerControlsVisible = !mediaPlayer.PlayerControlsVisible;

        if (!mediaPlayer.PlayerControlsVisible && !customLayout.IsSideChildVisible)
        {
            Controls.FullScreen();
        }
    }

    void OnPlayerSwiped(object sender, SwipedEventArgs e)
    {
        if (customLayout.Orientation == StackOrientation.Horizontal)
        {
            HandleHorizontalSwipe(e.Direction);
        }
        else if (customLayout.Orientation == StackOrientation.Vertical)
        {
            HandleVerticalSwipe(e.Direction);
        }
    }

    void HandleHorizontalSwipe(SwipeDirection direction)
    {
        if (direction == SwipeDirection.Left && !customLayout.IsSideChildVisible)
        {
            customLayout.IsSideChildVisible = true;
        }
        else if (direction == SwipeDirection.Right && customLayout.IsSideChildVisible)
        {
            customLayout.IsSideChildVisible = false;
            Controls.FullScreen();
        }
    }

    void HandleVerticalSwipe(SwipeDirection direction)
    {
        if (direction == SwipeDirection.Up && !customLayout.IsSideChildVisible)
        {
            customLayout.IsSideChildVisible = true;
            Controls.RestoreScreen();
        }
        else if (direction == SwipeDirection.Down && customLayout.IsSideChildVisible)
        {
            customLayout.IsSideChildVisible = false;
            Controls.FullScreen();
        }
    }
}