using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class EntryPopup : Popup<string>
{
    public EntryPopup(StringEntryPopupVm vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
