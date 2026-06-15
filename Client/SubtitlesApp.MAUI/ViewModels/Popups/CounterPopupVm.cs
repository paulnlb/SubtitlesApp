using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.Interfaces;

namespace SubtitlesApp.ViewModels.Popups;

public partial class CounterPopupVm(ICustomPopupService popupService) : BasePopupVm, IQueryAttributable
{
    [ObservableProperty]
    private int _min;

    [ObservableProperty]
    private int _max = int.MaxValue;

    [ObservableProperty]
    private int _counter;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(Title), out var titleValue);
        query.TryGetValue(nameof(AcceptText), out var acceptTextValue);
        query.TryGetValue(nameof(CancelText), out var cancelTextValue);
        query.TryGetValue(nameof(Counter), out var counterValue);
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
        if (counterValue is int counter)
        {
            Counter = counter;
        }
        if (minValue is int min)
        {
            Min = min;
        }
        if (maxValue is int max)
        {
            Max = max;
        }

        query.Clear();
    }

    public override Task Accept()
    {
        return popupService.CloseCurrentAsync(Counter);
    }

    public override Task Cancel()
    {
        return popupService.CloseCurrentAsync();
    }

    [RelayCommand]
    public void Increment() => Counter = Math.Min(Max, Counter + 1);

    [RelayCommand]
    public void Decrement() => Counter = Math.Max(Min, Counter - 1);
}
