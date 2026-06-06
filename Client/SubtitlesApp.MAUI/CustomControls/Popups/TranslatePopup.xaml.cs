using CommunityToolkit.Maui.Views;
using SubtitlesApp.ClientModels;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.CustomControls.Popups;

public partial class TranslatePopup : Popup<TranslationSettings>
{
    public TranslatePopup(TranslatePopupViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
