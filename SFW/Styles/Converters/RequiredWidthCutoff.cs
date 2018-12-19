using System;
using System.Windows.Data;

namespace SFW.Styles.Converters
{
    public sealed class RequiredWidthCutoff : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int.TryParse(value.ToString(), out int result);
            return value.ToString().Equals("NaN") ? true : result > 70;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
}
