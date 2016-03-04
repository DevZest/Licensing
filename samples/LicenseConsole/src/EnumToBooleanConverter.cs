using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace LicenseConsole
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type enumType = value.GetType();
            string strParam = parameter as string;
            if (strParam == null)
                return DependencyProperty.UnsetValue;

            object valueParam = Enum.Parse(enumType, strParam);
            if (enumType.IsDefined(typeof(FlagsAttribute), false) && (int)valueParam != 0)
                return ((int)value & (int)valueParam) == (int)valueParam;
            else
                return valueParam.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strParam = parameter as string;
            if (strParam == null)
                return DependencyProperty.UnsetValue;
            return Enum.Parse(targetType, strParam);
        }
    }
}