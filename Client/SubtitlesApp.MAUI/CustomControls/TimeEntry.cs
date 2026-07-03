namespace SubtitlesApp.CustomControls;

using System.ComponentModel;
using Microsoft.Maui.Controls;

public class TimeEntry : Entry
{
    public TimeEntry()
    {
        Keyboard = Keyboard.Numeric;

        if (string.IsNullOrEmpty(Text))
        {
            Text = "00:00:00";
        }

        TextChanged += OnTimeTextChanged;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnTimeTextChanged(object? sender, TextChangedEventArgs e)
    {
        try
        {
            string rawDigits = (e.NewTextValue ?? "").Replace(":", "");

            rawDigits = new string(rawDigits.Where(char.IsDigit).ToArray());

            if (rawDigits.Length > 6)
            {
                rawDigits = rawDigits.Substring(rawDigits.Length - 6);
            }
            else if (rawDigits.Length < 6)
            {
                rawDigits = rawDigits.PadLeft(6, '0');
            }

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

            UpdateText($"{hh:D2}:{mm:D2}:{ss:D2}");
        }
        catch
        {
            UpdateText("00:00:00");
        }
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
}
