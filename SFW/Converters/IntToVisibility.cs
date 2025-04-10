using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SFW.Converters
{
    public class IntToVisibility : IValueConverter, IMultiValueConverter
    {
        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter.ToString()[0] == 'L')
            {
                var _shift = parameter.ToString()[1].ToString();
                return _shift == value.ToString() ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (parameter != null && parameter.ToString().Contains("Ncr"))
            {
                var _temp = int.TryParse(value.ToString(), out int i) ? i : 0;
                switch (parameter.ToString())
                {
                    case "QmsHeader":
                        return _temp == 0 ? Visibility.Visible : Visibility.Collapsed;
                    case "QmsDetail":
                        return _temp == 0 ? Visibility.Hidden : Visibility.Visible;

                }
            }
            var _isInt = int.TryParse(value.ToString(), out int _val);
            var _parInt = int.TryParse(parameter?.ToString(), out int _par);
            if (_parInt && _isInt)
            {
                return _val == _par ? Visibility.Visible : Visibility.Collapsed; 
            }
            else if (parameter != null && _isInt)
            {
                return _val > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return _isInt ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Collapsed;
        }

        #endregion

        #region IMultiValueConverter Implementation

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && values?.Length != 0)
            {
                if (int.TryParse(values[0].ToString(), out int _id) && bool.TryParse(values[1].ToString(), out bool _isNew))
                {
                    switch (parameter.ToString())
                    {
                        case "NcrHeader":
                            return _id == 0 && _isNew == false ? Visibility.Visible : Visibility.Hidden;
                        case "NcrDetail":
                            return (_id == 0 && _isNew) || (_id > 0 && !_isNew) ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
