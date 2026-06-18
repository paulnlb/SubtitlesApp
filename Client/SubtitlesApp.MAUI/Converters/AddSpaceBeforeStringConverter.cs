using System.Globalization;

namespace SubtitlesApp.Converters;

public class AddSpaceBeforeStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text)
        {
            return Binding.DoNothing;
        }

        return " " + text;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
