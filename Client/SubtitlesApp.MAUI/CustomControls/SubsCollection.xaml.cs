using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.Extensions;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SubtitlesApp.CustomControls;

public partial class SubsCollection : ContentView
{
    private int _currentSubIndex = 0;
    private bool _autoScrollEnabled = true;

	public SubsCollection()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty SubsSourceProperty =
            BindableProperty.Create(nameof(SubsSource), typeof(ObservableCollection<Subtitle>), typeof(MediaPlayer), new ObservableCollection<Subtitle>());

    public static readonly BindableProperty CurrentTimePositionProperty = 
            BindableProperty.Create(nameof(CurrentTimePosition), typeof(TimeSpan), typeof(MediaPlayer), TimeSpan.Zero, propertyChanged: OnCurrentTimePositionChanged);

    public static readonly BindableProperty IsScrollButtonVisibleProperty = 
        BindableProperty.Create(nameof(IsScrollButtonVisible), typeof(bool), typeof(MediaPlayer), false);

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

    public bool IsScrollButtonVisible
    {
        get => (bool)GetValue(IsScrollButtonVisibleProperty);
        set => SetValue(IsScrollButtonVisibleProperty, value);
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

            var(sub, index) = subsCollection.SubsSource.BinarySearch(newPosition);

            if (sub != null)
            {
                subsCollection.subsCollectionView.SelectedItem = sub;
                subsCollection._currentSubIndex = index;
            }
        }
    }

    async void OnSubSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_autoScrollEnabled || SubsSource is null || SubsSource.Count == 0)
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

    void OnScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if(e.FirstVisibleItemIndex > _currentSubIndex || e.LastVisibleItemIndex < _currentSubIndex)
        {
            _autoScrollEnabled = false;
            IsScrollButtonVisible = true;
        }
        else
        {
            _autoScrollEnabled = true;
            IsScrollButtonVisible = false;
        }
    }

    void OnSubTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Subtitle sub)
        {
            SubtileTapped?.Invoke(this, new SubtitleTappedEventArgs(sub));
        }
    }

    async void OnScrollToCurrentClicked(object sender, EventArgs e)
    {
        _autoScrollEnabled = true;

        if (subsCollectionView.SelectedItem is Subtitle subtitle)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await TryScrollToSub(subtitle);
            });
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