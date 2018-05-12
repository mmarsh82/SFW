﻿using System;
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
                return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                return string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Hidden : Visibility.Visible;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Visibility.Visible;
        }
    }
}
