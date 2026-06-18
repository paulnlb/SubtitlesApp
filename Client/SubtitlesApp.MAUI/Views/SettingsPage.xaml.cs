using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModelNew vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}
