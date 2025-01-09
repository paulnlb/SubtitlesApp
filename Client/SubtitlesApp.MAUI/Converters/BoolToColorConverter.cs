using System.Globalization;

namespace SubtitlesApp.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var defaultColor = Colors.Transparent;
        if (value == null)
        {
            return defaultColor;
        }

        if (!targetType.IsAssignableFrom(typeof(Color)))
        {
            return null;
        }

        var isHighlighted = (bool)value;

        return isHighlighted ? Colors.Grey : defaultColor;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
