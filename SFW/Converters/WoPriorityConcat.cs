using System;
using System.Globalization;
using System.Windows.Data;

//Created by Michael Marsh 9-26-18

namespace SFW.Converters
{
    public sealed class WoPriorityConcat : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{value}*{parameter}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
