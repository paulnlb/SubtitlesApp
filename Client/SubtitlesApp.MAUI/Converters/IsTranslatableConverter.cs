using SubtitlesApp.Core.Models;
using System.Globalization;

namespace SubtitlesApp.Converters;

public class IsTranslatableConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 0)
        {
            return false;
        }

        if (values[0] is not bool isTranslated)
        {
            return false;
        }

        if (values[1] is not Translation translation)
        {
            return false;
        }

        return !isTranslated && translation is not null;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
