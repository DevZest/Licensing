using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace DevZest.Licensing
{
    internal static class ExceptionMessages
    {
        public static string FormatInvalidLicenseKey(string licenseKey)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Exception_InvalidLicenseKey, licenseKey);
        }

        public static string CanNotVerifySignedLicense
        {
            get { return SR.Exception_CanNotVerifySignedLicense; }
        }

        public static string FrozenAccess
        {
            get { return SR.Exception_FrozenAccess; }
        }

        public static string FormatDuplicateLicenseItemName(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Exception_DuplicateLicenseItemName, name);
        }

        public static string FormatEmptyLicenseItemName(int index)
        {
            return string.Format(CultureInfo.CurrentUICulture, SR.Exception_EmptyLicenseItemName, index);
        }

        public static string LicenseMustBeFrozenBeforeValidate
        {
            get { return SR.Exception_LicenseMustBeFrozenBeforeValidate; }
        }

        public static string FormatNullPublicKey(Assembly assembly)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Exception_NullPublicKey, AssemblyLicense.GetAssemblyName(assembly));
        }

        public static string FormatNullPrivateKey(string product)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Exception_NullPrivateKey, product);
        }

        public static string EmptyLicenseProviderResult
        {
            get { return SR.Exception_EmptyLicenseProviderResult; }
        }
    }
}
