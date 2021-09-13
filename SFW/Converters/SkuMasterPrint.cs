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
            var _rtnVal = string.Empty;
            foreach (var val in values)
            {
                _rtnVal += $"{val}*";
            }
            _rtnVal = _rtnVal.Substring(0, _rtnVal.Length - 1);
            return _rtnVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
