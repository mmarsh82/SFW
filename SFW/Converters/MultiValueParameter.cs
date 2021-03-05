using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SFW.Converters
{
    public class MultiValueParameter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var _rtnVal = "";
            if (!values.Contains(null))
            {
                foreach (var v in values)
                {
                    _rtnVal += v.ToString();
                    _rtnVal += values.Last() == v ? "" : "*";
                }
            }
            return _rtnVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
