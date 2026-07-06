using CommunityToolkit.Maui.Views;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Helpers;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class TranslatePopup : Popup<TranslationSettings>
{
    public TranslatePopup(TranslatePopupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        ViewSizeHelper.SetPopupSize(this);
    }
}
