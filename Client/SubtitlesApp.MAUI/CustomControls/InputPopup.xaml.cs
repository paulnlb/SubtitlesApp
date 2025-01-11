using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels;

namespace SubtitlesApp.CustomControls;

public partial class InputPopup : Popup
{
    public InputPopup(InputPopupViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
