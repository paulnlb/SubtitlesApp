using CommunityToolkit.Mvvm.ComponentModel;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public partial class DoubleEntryPopupVm(ICustomPopupService popupService) : BasePopupVm, IQueryAttributable
{
    [ObservableProperty]
    private double _min = double.MinValue;

    [ObservableProperty]
    private double _max = double.MaxValue;

    [ObservableProperty]
    private double _value;

    [ObservableProperty]
    private bool _isInputValid;

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
        if (valueObj is double value)
        {
            Value = value;
        }
        if (minValue is double min)
        {
            Min = min;
        }
        if (maxValue is double max)
        {
            Max = max;
        }

        query.Clear();
    }

    public override Task Accept()
    {
        return popupService.CloseCurrentAsync<double?>(Value);
    }

    public override Task Cancel()
    {
        return popupService.CloseCurrentAsync<double?>(null);
    }

    partial void OnValueChanged(double value)
    {
        IsAcceptEnabled = IsInputValid && value <= Max && value >= Min;
    }

    partial void OnIsInputValidChanged(bool value)
    {
        IsAcceptEnabled = value && Value <= Max && Value >= Min;
    }
}
