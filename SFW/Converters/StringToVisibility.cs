using System;
using System.Windows;
using System.Windows.Data;

//Created by Michael Marsh 5-1-18

namespace SFW.Converters
{
    public sealed class StringToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter?.ToString() == "i")
            {
                return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (parameter != null && parameter.ToString().Contains("~"))
            {
                return value.ToString() == parameter.ToString().Replace('~',' ').Trim() ? Visibility.Visible : Visibility.Collapsed;
            }
            else if(parameter?.ToString() == "Hide")
            {
                return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Hidden : Visibility.Visible;
            }
            else if(parameter?.ToString() == "Status")
            {
                return value?.ToString() == "O" ? Visibility.Collapsed : Visibility.Visible;
            }
            else if (parameter?.ToString() == "PriTime")
            {
                return value?.ToString() == "999" ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Visibility.Visible;
        }
    }
}
