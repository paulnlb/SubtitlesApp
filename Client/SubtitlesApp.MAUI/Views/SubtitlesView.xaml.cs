using SubtitlesApp.ClientModels;
using SubtitlesApp.ClientModels.EventArgs;

namespace SubtitlesApp.Views;

public partial class SubtitlesView : ContentView
{
    public event EventHandler<SeekEventArgs>? SeekRequested;

    public SubtitlesView()
    {
        InitializeComponent();
    }

    public void Cleanup()
    {
        subtitleCollection.Dispose();
        translationCollection.Dispose();
    }

    private void OnSubtitleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not VisualSubtitle subtitle)
        {
            return;
        }

        SeekRequested?.Invoke(this, new SeekEventArgs { Time = subtitle.TimeInterval.StartTime });
    }
}
