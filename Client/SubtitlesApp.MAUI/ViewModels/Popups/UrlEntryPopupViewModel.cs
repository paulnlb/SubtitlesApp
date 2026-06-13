using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitlesApp.ViewModels.Popups;

public partial class UrlEntryPopupViewModel(IPopupService popupService) : BasePopupVm, IQueryAttributable
{
    [ObservableProperty]
    private string _url;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(Title), out var titleValue);
        query.TryGetValue(nameof(AcceptText), out var acceptTextValue);
        query.TryGetValue(nameof(CancelText), out var cancelTextValue);

        if (titleValue is string title)
        {
            Title = title;
        }
        if (acceptTextValue is string acceptText)
        {
            AcceptText = acceptText;
        }
        if (cancelTextValue is string cancelText)
        {
            CancelText = cancelText;
        }

        query.Clear();
    }

    public override Task Accept()
    {
        return popupService.ClosePopupAsync(Shell.Current, Url);
    }

    public override Task Cancel()
    {
        return popupService.ClosePopupAsync(Shell.Current);
    }
}
