using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class UrlEntryPopup : Popup<string>
{
    public UrlEntryPopup(UrlEntryPopupVm vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
