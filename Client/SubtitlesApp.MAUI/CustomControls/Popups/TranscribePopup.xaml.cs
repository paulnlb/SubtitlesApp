using CommunityToolkit.Maui.Views;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.CustomControls.Popups;

public partial class TranscribePopup : Popup<TranscriptionSettings>
{
    public TranscribePopup(TranscribePopupViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
