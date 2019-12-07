using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SFW.Converters
{
    public class BoolToBorderColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(bool))
            {
                return (bool)value ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.Crimson);
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
