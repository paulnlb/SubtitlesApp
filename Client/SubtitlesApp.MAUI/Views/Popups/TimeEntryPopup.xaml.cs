using CommunityToolkit.Maui.Views;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class TimeEntryPopup : Popup<TimeSpan>
{
    public TimeEntryPopup(TimeEntryPopupVm vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}
