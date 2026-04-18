using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.CustomControls.Popups;

public partial class TranslatePopup : Popup
{
    public TranslatePopup(TranslatePopupViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
