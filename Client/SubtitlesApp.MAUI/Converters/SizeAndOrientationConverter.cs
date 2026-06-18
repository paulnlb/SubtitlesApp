using System.Globalization;
using SubtitlesApp.ClientModels.Enums;

namespace SubtitlesApp.Converters;

public class SizeAndOrientationConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length != 3 || values[0] == null || values[1] == null || values[2] == null)
            return Rect.Zero;

        var relWidth = System.Convert.ToDouble(values[0]);
        var relHeight = System.Convert.ToDouble(values[1]);
        var orientation = (AdaptiveLayoutOrientation)values[2];

        if (orientation == AdaptiveLayoutOrientation.Vertical)
        {
            return new Rect(0, 0, 1, relHeight);
        }
        else
        {
            return new Rect(0, 0, relWidth, 1);
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
