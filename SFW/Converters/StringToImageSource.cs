using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SFW.Converters
{
    public class StringToImageSource : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string valueString))
            {
                return null;
            }
            try
            {
                ImageSource image = BitmapFrame.Create(new Uri(valueString), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
                return image;
            }
            catch { return null; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
