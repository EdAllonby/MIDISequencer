﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Sequencer.View
{
    /// <summary>
    /// Support WPF selections based on enum values.
    /// </summary>
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}