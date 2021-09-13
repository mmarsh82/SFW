using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SFW.Converters
{
    public class ValueToMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _val = double.TryParse(value.ToString(), out double d) ? Math.Abs(d) : 0;
            switch (parameter.ToString())
            {
                case "R":
                    return new Thickness(_val * 15, 0, 0, 0);
                case "T":
                    return new Thickness(0, _val, 0, 0);
                case "L":
                    return new Thickness(0, 0, _val, 0);
                case "B":
                    return new Thickness(0, 0, 0, _val);
                default:
                    return new Thickness(_val, _val, _val, _val);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness(0, 0, 0, 0);
        }
    }
}
