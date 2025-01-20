using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.CustomControls.Popups;

public partial class LoadingPopup : Popup
{
    public LoadingPopup(LoadingPopupViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
