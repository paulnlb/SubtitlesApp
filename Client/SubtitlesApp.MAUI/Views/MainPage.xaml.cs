using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}
