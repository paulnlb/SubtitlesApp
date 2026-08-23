using System.Globalization;

namespace SubtitlesApp.Converters;

internal class ContainsAnyConverter<T> : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ICollection<T> collection)
        {
            return false;
        }

        return collection.Count > 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
