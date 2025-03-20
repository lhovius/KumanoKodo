using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace KumanoKodo.Converters;

public class IndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Button button && button.Parent is Panel panel)
        {
            return panel.Children.IndexOf(button);
        }
        return -1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 