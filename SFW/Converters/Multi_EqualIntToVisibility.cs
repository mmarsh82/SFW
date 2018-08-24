using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

//Created by Michael Marsh 8-24-18

namespace SFW.Converters
{
    public class Multi_EqualIntToVisibility : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var _test = true;
            var _total = 0; var _count = 0;
            foreach (object o in values)
            {
                var i = System.Convert.ToInt32(o);
                _test = _total == 0 ? true : i * _count == _total;
                _total += i;
                _count++;
            }
            return !_test ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
