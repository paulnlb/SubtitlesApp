using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels;

namespace SubtitlesApp.CustomControls;

public partial class LoadingPopup : Popup
{
    public LoadingPopup(LoadingPopupViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
