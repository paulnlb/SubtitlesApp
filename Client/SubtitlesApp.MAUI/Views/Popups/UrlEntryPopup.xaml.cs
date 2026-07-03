using CommunityToolkit.Maui.Views;
using SubtitlesApp.Helpers;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class UrlEntryPopup : Popup<string>
{
    public UrlEntryPopup(UrlEntryPopupVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
        ViewSizeHelper.SetPopupSize(this);
    }
}
