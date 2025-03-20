using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KumanoKodo.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException($"Target type must be {typeof(Visibility)}");

            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("NullToVisibilityConverter only supports one-way conversion");
        }
    }
} 