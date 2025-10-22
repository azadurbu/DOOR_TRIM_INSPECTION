using System;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;

namespace DOOR_TRIM_INSPECTION.Class
{
    public class HexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrEmpty(hex))
            {
                if (!hex.StartsWith("#"))
                {
                    hex = "#" + hex;
                }
                try
                {
                    return (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
                }
                catch (FormatException fex)
                {
                    return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
