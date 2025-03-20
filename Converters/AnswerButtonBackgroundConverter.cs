using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace KumanoKodo.Converters;

public class AnswerButtonBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int selectedIndex && parameter is int currentIndex)
        {
            if (selectedIndex == currentIndex)
            {
                return new SolidColorBrush(Colors.LightBlue);
            }
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 