using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public partial class EntryPopupViewModel<T>(ICustomPopupService popupService) : BasePopupVm, IQueryAttributable
{
    [ObservableProperty]
    private T _value;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(Title), out var titleValue);
        query.TryGetValue(nameof(AcceptText), out var acceptTextValue);
        query.TryGetValue(nameof(CancelText), out var cancelTextValue);
        query.TryGetValue(nameof(Value), out var valueObj);

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
        if (valueObj is T value)
        {
            Value = value;
        }

        query.Clear();
    }

    public override Task Accept()
    {
        return popupService.CloseCurrentAsync(Value);
    }

    public override Task Cancel()
    {
        return popupService.CloseCurrentAsync();
    }
}
