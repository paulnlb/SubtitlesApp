using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.CustomControls.Popups;

public partial class SubtitlesSettingsPopup : Popup
{
    public SubtitlesSettingsPopup(SubtitlesSettingsPopupViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
