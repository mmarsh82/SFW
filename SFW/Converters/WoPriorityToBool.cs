using System;
using System.Globalization;
using System.Windows.Data;

//Created by Michael Marsh 9-25-18

namespace SFW.Converters
{
    public sealed class WoPriorityToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter != null && parameter.ToString() == value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
