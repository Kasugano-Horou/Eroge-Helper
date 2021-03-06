﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ErogeHelper.Common.Converter
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        // return Visibility -> ElementType
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        // return bool -> Binding Path Type
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }
}
