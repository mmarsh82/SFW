using System;
using System.Globalization;
using System.Windows.Data;

//Created by Michael Marsh 5-8-18

namespace SFW.Converters
{
    public class WorkOrderNumber : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter?.ToString() == "Seq" ? value.ToString().Substring(value.ToString().IndexOf('*') + 1) : value.ToString().Substring(0, value.ToString().IndexOf('*'));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
