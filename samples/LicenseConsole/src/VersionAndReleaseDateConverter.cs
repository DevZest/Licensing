using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using DevZest.Licensing;

namespace LicenseConsole
{
    public class VersionAndReleaseDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Version version = value as Version;

            if (version == null)
                return DependencyProperty.UnsetValue;

            DateTime releaseDate = AssemblyInfo.GetReleaseDate(version);
            string strReleaseDate = releaseDate.Year.ToString("0000", CultureInfo.InvariantCulture) + "/"
                + releaseDate.Month.ToString("00", CultureInfo.InvariantCulture) + "/"
                + releaseDate.Day.ToString("00", CultureInfo.InvariantCulture);

            return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", version, strReleaseDate);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}