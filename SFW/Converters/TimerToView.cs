using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SFW.Converters
{
    public class TimerToView : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (char.TryParse(parameter.ToString(), out char c))
            {
                switch (c)
                {
                    case 'P':
                        return ApplicationTimer.Status == TimerState.Paused ? Visibility.Visible : Visibility.Collapsed;
                    case 'A':
                        return ApplicationTimer.Status != TimerState.Paused ? Visibility.Visible : Visibility.Collapsed;
                    case 'C':
                        switch (ApplicationTimer.Status)
                        {
                            case TimerState.Aborted:
                            case TimerState.Stopped:
                                return new SolidColorBrush(Colors.Crimson);
                            case TimerState.Sleeping:
                                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF29D129"));
                            case TimerState.Running:
                                return new SolidColorBrush(Colors.Orange);
                            default:
                                return new SolidColorBrush(Colors.Black);
                        }
                    default:
                        return Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Visibility.Collapsed;
        }
    }
}
