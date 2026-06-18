using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public class StringEntryPopupVm(ICustomPopupService popupService) : EntryPopupViewModel<string>(popupService) { }
