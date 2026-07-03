using System.ComponentModel;
using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.CustomControls;

public class TimeEntry : Entry
{
    public static readonly BindableProperty TimeScopeProperty = BindableProperty.Create(
        nameof(TimeScope),
        typeof(TimeEntryScope),
        typeof(TimeEntry),
        TimeEntryScope.Hours
    );

    public TimeEntryScope TimeScope
    {
        get => (TimeEntryScope)GetValue(TimeScopeProperty);
        set => SetValue(TimeScopeProperty, value);
    }

    private int _digitsCount;
    private string _defaultText;

    public TimeEntry()
    {
        Keyboard = Keyboard.Numeric;
        _digitsCount = GetDigitsCount();
        _defaultText = GetDefaultText();

        if (string.IsNullOrEmpty(Text))
        {
            Text = _defaultText;
        }

        TextChanged += OnTimeTextChanged;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnTimeTextChanged(object? sender, TextChangedEventArgs e)
    {
        _digitsCount = GetDigitsCount();
        _defaultText = GetDefaultText();

        try
        {
            string rawDigits = (e.NewTextValue ?? "").Replace(":", "");

            rawDigits = new string(rawDigits.Where(char.IsDigit).ToArray());

            if (rawDigits.Length > _digitsCount)
            {
                rawDigits = rawDigits.Substring(rawDigits.Length - _digitsCount);
            }
            else if (rawDigits.Length < _digitsCount)
            {
                rawDigits = rawDigits.PadLeft(_digitsCount, '0');
            }

            var finalText = TimeScope switch
            {
                TimeEntryScope.Hours => FormatTimeHours(rawDigits),
                TimeEntryScope.Minutes => FormatTimeMinutes(rawDigits),
            };

            UpdateText(finalText);
        }
        catch
        {
            UpdateText(_defaultText);
        }
    }

    private static string FormatTimeHours(string rawDigits)
    {
        string hhStr = rawDigits.Substring(0, 2);
        string mmStr = rawDigits.Substring(2, 2);
        string ssStr = rawDigits.Substring(4, 2);

        int hh = int.Parse(hhStr);
        int mm = int.Parse(mmStr);
        int ss = int.Parse(ssStr);

        if (mm > 59)
            mm = 59;
        if (ss > 59)
            ss = 59;

        return $"{hh:D2}:{mm:D2}:{ss:D2}";
    }

    private static string FormatTimeMinutes(string rawDigits)
    {
        string mmStr = rawDigits.Substring(0, 2);
        string ssStr = rawDigits.Substring(2, 2);

        int mm = int.Parse(mmStr);
        int ss = int.Parse(ssStr);

        if (mm > 59)
            mm = 59;
        if (ss > 59)
            ss = 59;

        return $"{mm:D2}:{ss:D2}";
    }

    private void UpdateText(string formattedTime)
    {
        Dispatcher.Dispatch(() =>
        {
            TextChanged -= OnTimeTextChanged;
            Text = formattedTime;
            TextChanged += OnTimeTextChanged;
        });
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

    private string GetDefaultText()
    {
        return TimeScope switch
        {
            TimeEntryScope.Hours => "00:00:00",
            TimeEntryScope.Minutes => "00:00",
        };
    }
}
