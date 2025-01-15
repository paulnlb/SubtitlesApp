using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.CustomControls.Popups;

public partial class TranslationSettingsPopup : Popup
{
    public TranslationSettingsPopup(TranslationSettingsPopupViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
