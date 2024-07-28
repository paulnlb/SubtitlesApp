using CommunityToolkit.Maui.Views;
using SubtitlesApp.Core.Enums;
using SubtitlesApp.Core.Models;
using SubtitlesApp.ViewModels;
using System.ComponentModel;
using System.Diagnostics;

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

        viewModel.PropertyChanged += ViewModel_CurrentSubChanged;
    }

    async void ViewModel_CurrentSubChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not MediaElementViewModel vm)
        {
            return;
        }

        if (e.PropertyName == nameof(vm.CurrentSubtitleIndex) && vm.CurrentSubtitleIndex != -1)
        {
            await MainThread.InvokeOnMainThreadAsync(async() =>
            {
                await TryScrollToSub(vm.CurrentSubtitleIndex);
            });
        }
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

    async Task TryScrollToSub(int index, int attemptsNumber = 2)
    {
        Exception? exception = null;

        while (attemptsNumber > 0)
        {
            try
            {
                SubsCollection.ScrollTo(index);

                exception = null;
                break;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"\"{ex.Message}\" was caught. Trying to scroll again. Attemts remaining: {attemptsNumber}");
                exception = ex;
            }

            await Task.Delay(200);

            attemptsNumber--;
        }

        if (exception != null)
        {
            throw exception;
        }
    }
}