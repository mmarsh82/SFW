using SFW.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

//Created by Michael Marsh 5-3-18

namespace SFW.Converters
{
    public class WorkCenterNameConverter : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return value != null && value != DependencyProperty.UnsetValue
                        ? $"{Machine.GetMachineName(value.ToString())} ({value})"
                        : string.Empty;
            }
            catch
            {
                return "Error in load";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
}
