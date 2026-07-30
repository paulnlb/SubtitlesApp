using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class LogsPage : ContentPage
{
    public LogsPage(LogsPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private async void OnScrollBtnClicked(object sender, EventArgs e)
    {
        await scrollView.ScrollToAsync(0, logText.Height, true);
    }
}
