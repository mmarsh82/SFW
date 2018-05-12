using System;
using System.Globalization;
using System.Windows.Data;

//Created by Michael Marsh 5-8-18

namespace SFW.Converters
{
    public class SkuMasterPrint : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{values[0]}*{values[1]}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
