using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class UrlEntryPopup : Popup<string>
{
    public UrlEntryPopup(EntryPopupViewModel<string> vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
