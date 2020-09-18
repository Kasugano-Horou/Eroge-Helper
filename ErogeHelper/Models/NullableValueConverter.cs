using System;
using System.Globalization;
using System.Windows.Data;

namespace ErogeHelper.Models
{
    class NullableValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.IsNaN((double)value))
            {
                return null;
            }

            return value;
        }
    }
}
