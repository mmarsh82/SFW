using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SFW.Converters
{
    public class BoolToBackgroundColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value.ToString(), out bool b))
            {
                return b ? new SolidColorBrush(Colors.Crimson) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF135185"));
            }
            else if (parameter != null && parameter.ToString() == "Y")
            {
                return value.ToString() == "Y" ? new SolidColorBrush(Colors.Crimson) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF135185"));
            }
            else
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush(Colors.Gray);
        }
    }
}
