using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitlesApp.ClientModels.Enums;
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

    [ObservableProperty]
    private string _hourSection;

    [ObservableProperty]
    private string _minuteSection;

    [ObservableProperty]
    private string _secondSection;

    [ObservableProperty]
    private TimeEntryScope _timeScope = TimeEntryScope.Hours;

    private StringBuilder _timeString = new();

    public event EventHandler? TimeScopeChanged;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(Title), out var titleValue);
        query.TryGetValue(nameof(AcceptText), out var acceptTextValue);
        query.TryGetValue(nameof(CancelText), out var cancelTextValue);
        query.TryGetValue(nameof(Value), out var valueObj);
        query.TryGetValue(nameof(Min), out var minValue);
        query.TryGetValue(nameof(Max), out var maxValue);
        query.TryGetValue(nameof(TimeScope), out var timeScopeValue);

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
        if (timeScopeValue is TimeEntryScope timeScope)
        {
            TimeScope = timeScope;
        }

        SetSectionsFrom(Value);
        SetTimeStringFrom(Value);

        query.Clear();
    }

    [RelayCommand]
    public void Add(string value)
    {
        _timeString.Append(value);
        _timeString.Remove(0, value.Length);

        SetSectionsFrom(_timeString);
        RefreshValue();
    }

    [RelayCommand]
    public void Remove()
    {
        _timeString.Remove(_timeString.Length - 1, 1);
        _timeString.Insert(0, '0');

        SetSectionsFrom(_timeString);
        RefreshValue();
    }

    public override Task Accept()
    {
        TimeScopeChanged = null;

        return popupService.CloseCurrentAsync<TimeSpan?>(Value);
    }

    public override Task Cancel()
    {
        TimeScopeChanged = null;

        return popupService.CloseCurrentAsync<TimeSpan?>(null);
    }

    partial void OnValueChanged(TimeSpan value)
    {
        IsAcceptEnabled = value <= Max && value >= Min;
    }

    partial void OnTimeScopeChanged(TimeEntryScope value)
    {
        TimeScopeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetSectionsFrom(TimeSpan value)
    {
        HourSection = value.Hours.ToString("D2");
        MinuteSection = value.Minutes.ToString("D2");
        SecondSection = value.Seconds.ToString("D2");
    }

    private void SetSectionsFrom(StringBuilder value)
    {
        var cursor = 0;

        if (TimeScope == TimeEntryScope.Hours)
        {
            HourSection = value.ToString(cursor, 2);
            cursor += 2;
        }

        if (TimeScope >= TimeEntryScope.Minutes)
        {
            MinuteSection = value.ToString(cursor, 2);
            cursor += 2;
        }

        SecondSection = value.ToString(cursor, 2);
    }

    private void RefreshValue()
    {
        var hh = 0;
        var mm = 0;

        if (TimeScope == TimeEntryScope.Hours)
        {
            hh = int.Parse(HourSection);
        }

        if (TimeScope >= TimeEntryScope.Minutes)
        {
            mm = int.Parse(MinuteSection);
        }

        var ss = int.Parse(SecondSection);

        Value = new TimeSpan(hh, mm, ss);
    }

    private void SetTimeStringFrom(TimeSpan value)
    {
        _timeString = new StringBuilder(value.ToString(GetFormat()));
    }

    private string GetFormat()
    {
        return TimeScope switch
        {
            TimeEntryScope.Hours => "hhmmss",
            TimeEntryScope.Minutes => "mmss",
            TimeEntryScope.Seconds => "ss",
        };
    }
}
