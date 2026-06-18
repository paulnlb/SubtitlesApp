using System.Globalization;
using CommunityToolkit.Maui.Views;

namespace SubtitlesApp.Converters;

public class PathToFileNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            return GetFileName(path);
        }
        else if (value is MediaSource source && !string.IsNullOrWhiteSpace(source.ToString()))
        {
            return GetFileName(source.ToString()!);
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private string GetFileName(string fullPath)
    {
        var path = Uri.UnescapeDataString(fullPath);
        return Path.GetFileName(path);
    }
}
