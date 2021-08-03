using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SFW.Converters
{
    public class ValueToImage : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _bVal = bool.TryParse(value.ToString(), out bool b) ? b : false;
            return _bVal 
                ? BitmapFrame.Create(new Uri("pack://application:,,,/Icons/Accept.ico"))
                : BitmapFrame.Create(new Uri("pack://application:,,,/Icons/Remove.ico"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BitmapFrame.Create(new Uri("pack://application:,,,/Icons/Remove.ico"));
        }

        #endregion
    }
}
