using System.Globalization;
using System.Text.RegularExpressions;

namespace SubtitlesApp.Converters;

public partial class TimeSpanConverter : IValueConverter
{
    [GeneratedRegex(@"^([01]\d|2[0-3]):([0-5]\d):([0-5]\d)$")]
    private static partial Regex TimeRegex();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is TimeSpan timeSpan ? timeSpan.ToString(@"hh\:mm\:ss") : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string time)
        {
            return Binding.DoNothing;
        }

        if (TimeRegex().IsMatch(time) && TimeSpan.TryParse(time, out TimeSpan timeSpan))
            return timeSpan;

        return Binding.DoNothing;
    }
}
