using SubtitlesApp.Core.Models;
using SubtitlesApp.CustomControls;
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

        mediaPlayer.SetBinding(MediaPlayer.DurationProperty, nameof(MediaElementViewModel.MediaDuration), BindingMode.OneWayToSource);

        viewModel.PropertyChanged += ViewModel_CurrentSubChanged;
    }

    async void OnSubSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SubsCollection.SelectedItem is Subtitle subtitle)
        {
            await mediaPlayer.SeekTo(subtitle.TimeInterval.StartTime, CancellationToken.None);

            SubsCollection.SelectedItem = null;
        }
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

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        var vm = (MediaElementViewModel)BindingContext;
        await vm.CleanAsync();
        mediaPlayer.Stop();
        mediaPlayer.DisconnectHandler();
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