using System.Globalization;

namespace SubtitlesApp.Converters;

public class EnumToIntConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            return System.Convert.ToInt32(enumValue);
        }

        throw new ArgumentException("Value must be an Enum type");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue && targetType.IsEnum)
        {
            return Enum.ToObject(targetType, intValue);
        }

        throw new ArgumentException("Value must be an integer and targetType must be an Enum");
    }
}
