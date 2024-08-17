using MauiPageFullScreen;
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

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
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

    void OnMediaElementSwiped(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Down && DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            Controls.FullScreen();
            mainGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            mainGrid.RowDefinitions[1].Height = new GridLength(0);
            mainGrid.RowDefinitions[2].Height = new GridLength(0);
        }

        if (e.Direction == SwipeDirection.Up && DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
        {
            Controls.RestoreScreen();
            mainGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Auto);
            mainGrid.RowDefinitions[1].Height = new GridLength(50);
            mainGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
        }

        if (e.Direction == SwipeDirection.Left && DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Landscape)
        {
            mainGrid.ColumnDefinitions[0].Width = new GridLength(2, GridUnitType.Star);
            mainGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        }

        if (e.Direction == SwipeDirection.Right && DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Landscape)
        {
            mainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            mainGrid.ColumnDefinitions[1].Width = new GridLength(0);
        }
    }

    void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        if (e.DisplayInfo.Orientation == DisplayOrientation.Landscape)
        {
            Controls.FullScreen();
            SetLandScapeLayout();
        }
        else
        {
            Controls.RestoreScreen();
            SetPortraitLayout();
        }
    }

    void SetLandScapeLayout()
    {
        mainGrid.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(50) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            };
        mainGrid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            };

        mainGrid.SetRowSpan(mediaPlayer, 2);
        mainGrid.SetColumn(mediaPlayer, 0);

        mainGrid.SetColumn(statusField, 1);
        mainGrid.SetRow(statusField, 0);

        mainGrid.SetColumn(subsCollection, 1);
        mainGrid.SetRow(subsCollection, 1);
    }

    void SetPortraitLayout()
    {
        mainGrid.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(50) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            };
        mainGrid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            };

        mainGrid.SetRow(mediaPlayer, 0);
        mainGrid.SetRowSpan(mediaPlayer, 1);

        mainGrid.SetRow(statusField, 1);

        mainGrid.SetRow(subsCollection, 2);
    }
}