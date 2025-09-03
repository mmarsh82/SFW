using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SFW.Converters
{
    public class BoolToForegroundColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value.ToString(), out bool b))
            {
                return b ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
            }
            else
            {
                return new SolidColorBrush(Colors.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush(Colors.Gray);
        }
    }
}
