using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}
