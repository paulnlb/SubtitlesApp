using SubtitlesApp.Core.Models;
using System.Globalization;

namespace SubtitlesApp.Converters;

internal class SubtitleToStartTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Subtitle subtitle)
        {
            return null;
        }

        return subtitle.TimeInterval.StartTime;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
