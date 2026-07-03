using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public partial class TimeEntryPopupVm(ICustomPopupService popupService) : BasePopupVm, IQueryAttributable
{
    [ObservableProperty]
    private TimeSpan _min = TimeSpan.Zero;

    [ObservableProperty]
    private TimeSpan _max = new TimeSpan(0, 59, 59);

    [ObservableProperty]
    private TimeSpan _value;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(Title), out var titleValue);
        query.TryGetValue(nameof(AcceptText), out var acceptTextValue);
        query.TryGetValue(nameof(CancelText), out var cancelTextValue);
        query.TryGetValue(nameof(Value), out var valueObj);
        query.TryGetValue(nameof(Min), out var minValue);
        query.TryGetValue(nameof(Max), out var maxValue);

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
        if (valueObj is TimeSpan value)
        {
            Value = value;
        }
        if (minValue is TimeSpan min)
        {
            Min = min;
        }
        if (maxValue is TimeSpan max)
        {
            Max = max;
        }

        query.Clear();
    }

    public override Task Accept()
    {
        return popupService.CloseCurrentAsync<TimeSpan?>(Value);
    }

    public override Task Cancel()
    {
        return popupService.CloseCurrentAsync<TimeSpan?>(null);
    }

    partial void OnValueChanged(TimeSpan value)
    {
        IsAcceptEnabled = value <= Max && value >= Min;
    }
}
