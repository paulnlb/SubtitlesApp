using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class SessionCachePage : ContentPage
{
    public SessionCachePage(SessionCacheVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
