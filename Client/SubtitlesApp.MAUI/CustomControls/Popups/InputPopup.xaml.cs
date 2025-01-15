using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.CustomControls.Popups;

public partial class InputPopup : Popup
{
    public InputPopup(InputPopupViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
