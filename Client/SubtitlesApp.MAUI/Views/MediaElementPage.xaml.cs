using CommunityToolkit.Maui.Views;
using SubtitlesApp.Core.Enums;
using SubtitlesApp.ViewModels;
using System.ComponentModel;

namespace SubtitlesApp.Views;

public partial class MediaElementPage : ContentPage
{
    public MediaElementPage(MediaElementViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        viewModel.PropertyChanged += ViewModel_SeekChanged;
        viewModel.PropertyChanged += ViewModel_PlayerStateChanged;

        MediaElement.SetBinding(MediaElement.DurationProperty, nameof(MediaElementViewModel.MediaDuration));
    }

    async void ViewModel_SeekChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not MediaElementViewModel vm)
        {
            return;
        }

        if (e.PropertyName == nameof(vm.LastSeekedPosition))
        {
            await MediaElement.SeekTo(vm.LastSeekedPosition, CancellationToken.None);

            // unselect the current subtitle
            SubsCollection.SelectedItem = null;
        }
    }

    void ViewModel_PlayerStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not MediaElementViewModel vm)
        {
            return;
        }

        if (e.PropertyName == nameof(vm.PlayerState))
        {
            switch (vm.PlayerState)
            {
                case MediaPlayerStates.Playing:
                    MediaElement.Play();
                    break;
                case MediaPlayerStates.Paused:
                    MediaElement.Pause();
                    break;
                case MediaPlayerStates.Stopped:
                    MediaElement.Stop();
                    break;
            }
        }
    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        var vm = (MediaElementViewModel)BindingContext;
        await vm.CleanAsync();
        MediaElement.Stop();
        MediaElement.Handler?.DisconnectHandler();
    }
}