using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SmartX.WPF
{
    /// <summary>
    /// Converts a bool (IsConnected) into a Brush color.
    /// true  -> Green (Online)
    /// false -> Red   (Offline)
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isConnected)
            {
                return isConnected
                    ? new SolidColorBrush(Color.FromRgb(0, 217, 165))   // Green
                    : new SolidColorBrush(Color.FromRgb(255, 107, 107)); // Red
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

   
    /// Converts a bool (IsConnected) into a text label.

    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isConnected)
            {
                return isConnected ? "Online" : "Offline";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}