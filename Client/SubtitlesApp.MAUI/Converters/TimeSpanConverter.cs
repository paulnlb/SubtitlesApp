using System.Globalization;

namespace SubtitlesApp.Converters;

public class TimeSpanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is TimeSpan timeSpan ? timeSpan.ToString(@"hh\:mm\:ss") : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (TimeSpan.TryParse(value as string, out TimeSpan timeSpan))
            return timeSpan;

        return Binding.DoNothing;
    }
}
