using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace DevZest.Licensing
{
    internal static class Messages
    {
        public static string NullLicense
        {
            get { return SR.Message_NullLicense; }
        }

        public static string FormatLicenseError(LicenseError licenseError)
        {
            Debug.Assert(licenseError != null);
            return string.Format(
                CultureInfo.CurrentUICulture,
                SR.Message_LicenseError,
                AssemblyLicense.GetAssemblyName(licenseError.Assembly),
                licenseError.Reason,
                licenseError.Message,
                licenseError.License == null ? string.Empty : licenseError.License.SignedString);
        }

        public static string FormatExpiredLicense(DateTime dateTime)
        {
            return string.Format(CultureInfo.CurrentUICulture, SR.Message_ExpiredLicense, dateTime.ToShortDateString());
        }

        public static string InvalidAssemblyData
        {
            get { return SR.Message_InvalidAssemblyData; }
        }

        public static string InvalidMachineData
        {
            get { return SR.Message_InvalidMachineData; }
        }

        public static string InvalidUserData
        {
            get { return SR.Message_InvalidUserData; }
        }

        public static string FormatInvalidProductName(string productName)
        {
            return string.Format(SR.Message_InvalidProductName, productName);
        }
        
        public static string FormatNoMatchingLicenseItem(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Message_NoMatchingLicenseItem, name);
        }

        public static string FormatUpgradeExpired(DateTime upgradeExpirationDate, DateTime releaseDate)
        {
            return string.Format(CultureInfo.InstalledUICulture, SR.Message_UpgradeExpired, upgradeExpirationDate.ToShortDateString(), releaseDate.ToShortDateString());
        }

        public static string FormatEmbeddedResourceNotFound(string resourceName)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Message_EmbeddedResourceNotFound, resourceName);
        }

        public static string FileNotFound
        {
            get { return SR.Message_FileNotFound; }
        }

        public static string EmptyFile
        {
            get { return SR.Message_EmptyFile; }
        }

        public static string FormatReadFileFailed(string errorMesssage)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Message_ReadFileFailed, errorMesssage);
        }

        public static string OpenRegistryKeyFailed
        {
            get { return SR.Message_OpenRegistryKeyFailed; }
        }

        public static string FormatReadRegistryFailed(string errorMessage)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Message_ReadResgistryFailed, errorMessage);
        }

        public static string FormatAssemblySignedWithSameKey(Assembly assembly)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Message_AssemblySignedWithSameKey, assembly);
        }

        public static string EmptyRegistryValue
        {
            get { return SR.Message_EmptyRegistryValue; }
        }
    }
}
