using System;
using System.Windows;
using System.Windows.Data;

//Created by Michael Marsh 5-1-18

namespace SFW.Converters
{
    public sealed class CountToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                return System.Convert.ToInt32(value) > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return Visibility.Hidden;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Visibility.Visible;
        }
    }
}
