using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class LogsPage : ContentPage
{
    public LogsPage(LogsPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
