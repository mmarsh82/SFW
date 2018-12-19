using System;
using System.Globalization;
using System.Windows.Data;

//Created by Michael Marsh 11-30-18

namespace SFW.Converters
{
    public sealed class EnumToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || parameter == null ? false : value.ToString().Equals(parameter.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || parameter == null || !bool.TryParse(value.ToString(), out bool vResult) || !vResult ? null : Enum.Parse(targetType, parameter.ToString());
        }
    }
}
