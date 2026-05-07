using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.CustomControls.Popups;

public partial class TranscribePopup : Popup
{
    public TranscribePopup(TranscribePopupViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
