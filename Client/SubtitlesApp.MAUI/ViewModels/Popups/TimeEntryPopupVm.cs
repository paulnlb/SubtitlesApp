using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public class TimeEntryPopupVm(ICustomPopupService popupService) : EntryPopupViewModel<TimeSpan>(popupService)
{
    public override Task Accept()
    {
        return PopupService.CloseCurrentAsync<TimeSpan?>(Value);
    }

    public override Task Cancel()
    {
        return PopupService.CloseCurrentAsync<TimeSpan?>(null);
    }
}
