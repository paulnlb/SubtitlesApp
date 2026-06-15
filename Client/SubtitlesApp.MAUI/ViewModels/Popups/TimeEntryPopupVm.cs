using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public class TimeEntryPopupVm(ICustomPopupService popupService) : EntryPopupViewModel<TimeSpan>(popupService) { }
