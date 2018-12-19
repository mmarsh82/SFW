using System;
using System.Windows;
using System.Windows.Data;

//Created by Michael Marsh 8-24-18

namespace SFW.Converters
{
    public class DateToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
            {
                return value != null && System.Convert.ToDateTime(value) < DateTime.Today ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                switch(parameter.ToString())
                {
                    case "N":
                        return value != null && System.Convert.ToDateTime(value) != DateTime.MinValue ? Visibility.Visible : Visibility.Collapsed;
                    case "Ni":
                        return value != null && System.Convert.ToDateTime(value) == DateTime.MinValue ? Visibility.Visible : Visibility.Collapsed;
                    default:
                        return Visibility.Collapsed;
                }
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Visibility.Visible;
        }
    }
}
