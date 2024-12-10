using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SFW.Converters
{
    public class StatusToColor : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                switch (value.ToString())
                {
                    case "Open":
                        return new SolidColorBrush(Colors.Crimson);
                    case "Closed":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF135185"));
                    default:
                        return new SolidColorBrush(Colors.Black);
                }
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }

        #endregion
    }
}
