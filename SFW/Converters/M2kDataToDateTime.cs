using System;
using System.Windows.Data;

namespace SFW.Converters
{
    public class M2kDataToDateTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var _tempValue = value.ToString().Split('*');
            return parameter?.ToString() == "Date" 
                ? new DateTime(1967, 12, 31).AddDays(System.Convert.ToInt32(_tempValue[0])) 
                : new DateTime(1967, 12, 31).AddDays(System.Convert.ToInt32(_tempValue[0])).AddSeconds(System.Convert.ToInt32(_tempValue[1]));
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 0;
        }
    }
}
