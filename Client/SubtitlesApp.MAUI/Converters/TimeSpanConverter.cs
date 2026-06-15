using System.Globalization;
using System.Text.RegularExpressions;
using SubtitlesApp.Constants;

namespace SubtitlesApp.Converters;

public partial class TimeSpanConverter : IValueConverter
{
    [GeneratedRegex(@"^([01]\d|2[0-3]):([0-5]\d):([0-5]\d)$")]
    private static partial Regex TimeRegex();

    [GeneratedRegex(@"^[0-5]\d:[0-5]\d$")]
    private static partial Regex MinutesTimeRegex();

    private const string DefaultFormat = @"hh\:mm\:ss";
    private const string MinutesFormat = @"mm\:ss";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string format;

        if (parameter is string mode && mode == TimeSpanConverterConstants.MinutesMode)
        {
            format = MinutesFormat;
        }
        else
        {
            format = DefaultFormat;
        }

        return value is TimeSpan timeSpan ? timeSpan.ToString(format, culture) : Binding.DoNothing;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string time)
        {
            return Binding.DoNothing;
        }

        Regex timeRegex;
        string format;

        if (parameter is string mode && mode == TimeSpanConverterConstants.MinutesMode)
        {
            timeRegex = MinutesTimeRegex();
            format = MinutesFormat;
        }
        else
        {
            timeRegex = TimeRegex();
            format = DefaultFormat;
        }

        if (timeRegex.IsMatch(time) && TimeSpan.TryParseExact(time, format, culture, out TimeSpan timeSpan))
            return timeSpan;

        return Binding.DoNothing;
    }
}
