using SubtitlesApp.Core.Models;
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
}