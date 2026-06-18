using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public class UrlEntryPopupVm(ICustomPopupService popupService) : EntryPopupViewModel<string>(popupService) { }
