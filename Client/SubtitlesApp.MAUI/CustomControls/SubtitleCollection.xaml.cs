using System.Collections.ObjectModel;
using Microsoft.Maui.Adapters;
using SubtitlesApp.ClientModels;

namespace SubtitlesApp.CustomControls;

public partial class SubtitleCollection : ContentView, IDisposable
{
    private bool _disposed = false;

    public static readonly BindableProperty SubtitlesProperty = BindableProperty.Create(
        nameof(Subtitles),
        typeof(ObservableCollection<VisualSubtitle>),
        typeof(SubtitleCollection),
        propertyChanged: SubtitlesPropertyChanged
    );

    public static readonly BindableProperty CurrentSubtitleIndexProperty = BindableProperty.Create(
        nameof(CurrentSubtitleIndex),
        typeof(int),
        typeof(SubtitleCollection),
        0,
        propertyChanged: OnCurrentIndexChanged
    );

    private static readonly BindableProperty SubtitlesAdapterProperty = BindableProperty.Create(
        nameof(SubtitlesAdapter),
        typeof(ObservableCollectionAdapter<VisualSubtitle>),
        typeof(SubtitleCollection)
    );

    private static readonly BindableProperty FirstVisibleSubtitleIndexProperty = BindableProperty.Create(
        nameof(FirstVisibleSubtitleIndex),
        typeof(int),
        typeof(SubtitleCollection),
        0
    );

    private static readonly BindableProperty LastVisibleSubtitleIndexProperty = BindableProperty.Create(
        nameof(LastVisibleSubtitleIndex),
        typeof(int),
        typeof(SubtitleCollection),
        0
    );

    private static readonly BindableProperty AutoScrollEnabledProperty = BindableProperty.Create(
        nameof(AutoScrollEnabled),
        typeof(bool),
        typeof(SubtitleCollection),
        false
    );

    private static readonly BindableProperty EmptyTextProperty = BindableProperty.Create(
        nameof(EmptyText),
        typeof(string),
        typeof(SubtitleCollection),
        string.Empty
    );

    public ObservableCollection<VisualSubtitle> Subtitles
    {
        get => (ObservableCollection<VisualSubtitle>)GetValue(SubtitlesProperty);
        set => SetValue(SubtitlesProperty, value);
    }

    public int CurrentSubtitleIndex
    {
        get => (int)GetValue(CurrentSubtitleIndexProperty);
        set => SetValue(CurrentSubtitleIndexProperty, value);
    }
    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public event EventHandler<TappedEventArgs>? SubtitleTapped;

    public ObservableCollectionAdapter<VisualSubtitle> SubtitlesAdapter
    {
        get => (ObservableCollectionAdapter<VisualSubtitle>)GetValue(SubtitlesAdapterProperty);
        set => SetValue(SubtitlesAdapterProperty, value);
    }

    public int FirstVisibleSubtitleIndex
    {
        get => (int)GetValue(FirstVisibleSubtitleIndexProperty);
        set => SetValue(FirstVisibleSubtitleIndexProperty, value);
    }

    public int LastVisibleSubtitleIndex
    {
        get => (int)GetValue(LastVisibleSubtitleIndexProperty);
        set => SetValue(LastVisibleSubtitleIndexProperty, value);
    }

    public bool AutoScrollEnabled
    {
        get => (bool)GetValue(AutoScrollEnabledProperty);
        set => SetValue(AutoScrollEnabledProperty, value);
    }

    private bool IsCurrentSubVisible =>
        CurrentSubtitleIndex <= LastVisibleSubtitleIndex && CurrentSubtitleIndex >= FirstVisibleSubtitleIndex;

    public SubtitleCollection()
    {
        InitializeComponent();

        AutoScrollEnabled = true;
    }

    #region implementation of IDisposable

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            SubtitlesAdapter?.Dispose();
        }

        _disposed = true;
    }

    #endregion

    private static void SubtitlesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is ObservableCollection<VisualSubtitle> newSubtitles)
        {
            var adapter = new ObservableCollectionAdapter<VisualSubtitle>(newSubtitles);
            bindable.SetValue(SubtitlesAdapterProperty, adapter);
        }
    }

    private static void OnCurrentIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SubtitleCollection subsCollection && oldValue is int oldIndex && newValue is int newIndex)
        {
            var currSub = subsCollection.Subtitles[oldIndex];
            var newSub = subsCollection.Subtitles[newIndex];
            currSub.IsHighlighted = false;
            newSub.IsHighlighted = true;

            if (subsCollection.AutoScrollEnabled)
            {
                subsCollection.subtitlesList.ScrollToIndex(newIndex);
            }
        }
    }

    private void OnScrollToCurentClicked(object? sender, EventArgs e)
    {
        if (!IsCurrentSubVisible)
        {
            subtitlesList.ScrollToIndex(CurrentSubtitleIndex);
        }

        AutoScrollEnabled = true;
    }

    private void OnSubtitleTapped(object? sender, TappedEventArgs e)
    {
        SubtitleTapped?.Invoke(sender, e);
    }

    private void OnSubsScrolled(object? sender, ScrolledEventArgs e)
    {
        AutoScrollEnabled = IsCurrentSubVisible;
    }
}
