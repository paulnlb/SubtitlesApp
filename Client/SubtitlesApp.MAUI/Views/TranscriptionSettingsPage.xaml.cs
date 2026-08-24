using SubtitlesApp.ViewModels;

namespace SubtitlesApp.Views;

public partial class TranscriptionSettingsPage : ContentPage
{
    public TranscriptionSettingsPage(TranscriptionSettingsVm vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}
