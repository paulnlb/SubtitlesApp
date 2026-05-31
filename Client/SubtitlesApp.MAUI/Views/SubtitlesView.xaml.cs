using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.EventArgs;

namespace SubtitlesApp.Views;

public partial class SubtitlesView : ContentView, IDisposable
{
    private bool _disposed = false;
    public event EventHandler<SeekEventArgs>? SeekRequested;

    public SubtitlesView()
    {
        InitializeComponent();
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
            subtitleCollection.Dispose();
            translationCollection.Dispose();
        }

        _disposed = true;
    }

    #endregion

    private void OnSubtitleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not VisualSubtitle subtitle)
        {
            return;
        }

        SeekRequested?.Invoke(this, new SeekEventArgs { Time = subtitle.TimeInterval.StartTime });
    }
}
