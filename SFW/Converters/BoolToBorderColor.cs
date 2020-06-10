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
            var _brdrClr = Colors.Gray;
            switch (parameter?.ToString())
            {
                case "B":
                    _brdrClr = Colors.Black;
                    break;
                default:
                    _brdrClr = Colors.Gray;
                    break;
            }
            if (value.GetType() == typeof(bool))
            {
                return (bool)value ? new SolidColorBrush(_brdrClr) : new SolidColorBrush(Colors.Crimson);
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
