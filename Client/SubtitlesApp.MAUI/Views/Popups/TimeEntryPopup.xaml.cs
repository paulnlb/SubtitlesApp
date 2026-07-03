using CommunityToolkit.Maui.Views;
using SubtitlesApp.Helpers;
using SubtitlesApp.ViewModels.Popups;

namespace SubtitlesApp.Views.Popups;

public partial class TimeEntryPopup : Popup<TimeSpan>
{
    public TimeEntryPopup(TimeEntryPopupVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
        ViewSizeHelper.SetPopupSize(this);
    }
}
