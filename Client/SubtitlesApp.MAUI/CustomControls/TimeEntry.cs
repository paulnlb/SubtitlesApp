using System.ComponentModel;
using System.Globalization;
using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.CustomControls;

public partial class TimeEntry : Entry
{
    private bool _isInternalTimeUpdate;
    private const string DefaultText = "00:00:00";

    public static readonly BindableProperty TimeScopeProperty = BindableProperty.Create(
        nameof(TimeScope),
        typeof(TimeEntryScope),
        typeof(TimeEntry),
        TimeEntryScope.Hours,
        propertyChanged: OnTimeScopeChanged
    );

    public TimeEntryScope TimeScope
    {
        get => (TimeEntryScope)GetValue(TimeScopeProperty);
        set => SetValue(TimeScopeProperty, value);
    }

    public static readonly BindableProperty TimeValueProperty = BindableProperty.Create(
        nameof(TimeValue),
        typeof(TimeSpan),
        typeof(TimeEntry),
        TimeSpan.Zero,
        propertyChanged: OnTimeValueChanged
    );

    public TimeSpan TimeValue
    {
        get => (TimeSpan)GetValue(TimeValueProperty);
        set => SetValue(TimeValueProperty, value);
    }

    public TimeEntry()
    {
        Keyboard = Keyboard.Numeric;

        if (string.IsNullOrEmpty(Text))
        {
            Text = DefaultText;
        }

        TextChanged += OnTextChanged;
        PropertyChanged += OnPropertyChanged;
    }

    private static void OnTimeValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not TimeEntry timeEntry || newValue is not TimeSpan newTime)
        {
            return;
        }

        if (!timeEntry._isInternalTimeUpdate)
        {
            timeEntry.UpdateTextInternal(newTime.ToString(timeEntry.GetFormat(), CultureInfo.CurrentCulture));
        }
    }

    private static void OnTimeScopeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not TimeEntry timeEntry || newValue is not TimeEntryScope newScope)
        {
            return;
        }

        if (newScope != TimeEntryScope.Hours && timeEntry.Text == DefaultText)
        {
            timeEntry.UpdateTextInternal(TimeSpan.Zero.ToString(timeEntry.GetFormat()));
        }
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        try
        {
            var digitsCount = GetDigitsCount();

            string rawDigits = (e.NewTextValue ?? "").Replace(":", "");

            rawDigits = new string(rawDigits.Where(char.IsDigit).ToArray());

            if (rawDigits.Length > digitsCount)
            {
                rawDigits = rawDigits.Substring(rawDigits.Length - digitsCount);
            }
            else if (rawDigits.Length < digitsCount)
            {
                rawDigits = rawDigits.PadLeft(digitsCount, '0');
            }

            var (time, timeStr) = TimeScope switch
            {
                TimeEntryScope.Hours => ConvertToTimeHours(rawDigits),
                TimeEntryScope.Minutes => ConvertToTimeMinutes(rawDigits),
            };

            UpdateTextInternal(timeStr);
            UpdateTimeValueInternal(time);
        }
        catch
        {
            TimeValue = TimeSpan.Zero;
        }
    }

    private static (TimeSpan Time, string TimeStr) ConvertToTimeHours(string rawDigits)
    {
        string hhStr = rawDigits.Substring(0, 2);
        string mmStr = rawDigits.Substring(2, 2);
        string ssStr = rawDigits.Substring(4, 2);

        int hh = int.Parse(hhStr);
        int mm = int.Parse(mmStr);
        int ss = int.Parse(ssStr);

        return (new TimeSpan(hh, mm, ss), $"{hh:D2}:{mm:D2}:{ss:D2}");
    }

    private static (TimeSpan Time, string TimeStr) ConvertToTimeMinutes(string rawDigits)
    {
        string mmStr = rawDigits.Substring(0, 2);
        string ssStr = rawDigits.Substring(2, 2);

        int mm = int.Parse(mmStr);
        int ss = int.Parse(ssStr);

        return (new TimeSpan(0, mm, ss), $"{mm:D2}:{ss:D2}");
    }

    private void UpdateTextInternal(string formattedTime)
    {
        Dispatcher.Dispatch(() =>
        {
            TextChanged -= OnTextChanged;
            Text = formattedTime;
            TextChanged += OnTextChanged;
        });
    }

    private void UpdateTimeValueInternal(TimeSpan newTime)
    {
        _isInternalTimeUpdate = true;
        TimeValue = newTime;
        _isInternalTimeUpdate = false;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CursorPosition))
        {
            return;
        }

        if (CursorPosition != Text.Length)
        {
            CursorPosition = Text.Length;
        }
    }

    private int GetDigitsCount()
    {
        return TimeScope switch
        {
            TimeEntryScope.Hours => 6,
            TimeEntryScope.Minutes => 4,
        };
    }

    private string GetFormat()
    {
        return TimeScope switch
        {
            TimeEntryScope.Hours => @"hh\:mm\:ss",
            TimeEntryScope.Minutes => @"mm\:ss",
        };
    }
}
