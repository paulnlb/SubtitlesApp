using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.Extensions;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SubtitlesApp.CustomControls;

public partial class SubsCollection : ContentView
{
	public SubsCollection()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty SubsSourceProperty =
            BindableProperty.Create(nameof(SubsSource), typeof(ObservableCollection<Subtitle>), typeof(MediaPlayer), new ObservableCollection<Subtitle>());

    public static readonly BindableProperty CurrentTimePositionProperty = 
            BindableProperty.Create(nameof(CurrentTimePosition), typeof(TimeSpan), typeof(MediaPlayer), TimeSpan.Zero, propertyChanged: OnCurrentTimePositionChanged);

    public ObservableCollection<Subtitle> SubsSource
	{
        get => (ObservableCollection<Subtitle>)GetValue(SubsSourceProperty);
        set => SetValue(SubsSourceProperty, value);
    }

    public TimeSpan CurrentTimePosition
    {
        get => (TimeSpan)GetValue(CurrentTimePositionProperty);
        set => SetValue(CurrentTimePositionProperty, value);
    }

    public event EventHandler<SubtitleTappedEventArgs>? SubtileTapped;

    static void OnCurrentTimePositionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SubsCollection subsCollection)
        {
            var newPosition = (TimeSpan)newValue;

            if (subsCollection.subsCollectionView.SelectedItem is Subtitle currentSub 
                && currentSub.TimeInterval.ContainsTime(newPosition))
            {
                return;
            }

            (var sub, _) = subsCollection.SubsSource.BinarySearch(newPosition);

            if (sub != null)
            {
                subsCollection.subsCollectionView.SelectedItem = sub;
            }
        }
    }

    async void OnSubSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SubsSource is null)
        {
            return;
        }

        if (e.CurrentSelection.FirstOrDefault() is Subtitle subtitle)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await TryScrollToSub(subtitle);
            });
        }
    }

    void OnSubTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Subtitle sub)
        {
            SubtileTapped?.Invoke(this, new SubtitleTappedEventArgs(sub));
        }
    }

    async Task TryScrollToSub(Subtitle sub, int attemptsNumber = 2)
    {
        Exception? exception = null;

        while (attemptsNumber > 0)
        {
            try
            {
                subsCollectionView.ScrollTo(sub);

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

public class SubtitleTappedEventArgs : EventArgs
{
    public Subtitle Subtitle { get; }

    public SubtitleTappedEventArgs(Subtitle subtitle)
    {
        Subtitle = subtitle;
    }
}