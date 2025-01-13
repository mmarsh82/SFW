using Org.BouncyCastle.Asn1.X509.Qualified;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SFW.Converters
{
    public class SelectedRevision : IValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value.ToString(), out bool _bRef))
            {
                switch (targetType.Name)
                {
                    case "Boolean":
                        return _bRef;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
