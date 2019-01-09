using System;
using System.Windows.Data;

//Created by Michael Marsh 9-25-18

namespace SFW.Converters
{
    public sealed class SiteNumberToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch(parameter.ToString())
            {
                case "CSI":
                    return System.Convert.ToInt32(value) == 0;
                case "WCCO":
                    return System.Convert.ToInt32(value) == 1;
                default:
                    return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
}
