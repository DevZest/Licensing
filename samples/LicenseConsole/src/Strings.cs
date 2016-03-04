using System;
using System.Diagnostics;
using System.Globalization;
using DevZest.Licensing;

namespace LicenseConsole
{
    internal enum ExceptionId
    {
        NoAssemblyProductAttribute
    }

    internal enum MessageId
    {
        InvalidUpgradeExpirationDate,
    }

    internal enum CaptionId
    {
        OverwriteDistributableLicense
    }

    internal static class Strings
    {
        public static string FormatRegistryKeyNotExist(string registryKey)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Format_RegistryKeyNotExist, "HKLM\\" + registryKey);
        }

        public static string FormatMainWindowTitle(string productTitle)
        {
            return string.Format(CultureInfo.CurrentCulture, SR.Format_MainWindowTitle, productTitle);
        }

        public static string FormatLaunchEmailClientErrorMessage(Exception exception)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Format_LaunchEmailClientFailed, exception.Message);
        }

        public static string FormatDistributableLicenseSaved(string licenseFileName, string folderName)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Format_DistributableLicenseSaved, licenseFileName, folderName);
        }

        public static string FormatLicenseStateDescription(LicenseState state, License license)
        {
            switch (state)
            {
                case LicenseState.NotLicensed:
                    return SR.Format_LicenseState_NotLicensed;

                case LicenseState.FreeFeature:
                    return SR.Format_LicenseState_FreeFeature;

                case LicenseState.Evaluation:
                    return string.Format(CultureInfo.CurrentUICulture, SR.Format_LicenseState_Evaluation, license.Expiration);

                case LicenseState.EvaluationExpired:
                    return SR.Format_LicenseState_EvaluationExpired;
                
                default:
                    Debug.Assert(state == LicenseState.Paid);
                    return SR.Format_LicenseState_Paid;
            }
        }

        public static string GetExceptionDescription(ExceptionId exceptionId)
        {
            switch (exceptionId)
            {
                case ExceptionId.NoAssemblyProductAttribute:
                    return SR.Exception_NoAssemblyProductAttribute;
            }

            Debug.Assert(false);
            return null;
        }

        public static string FormatOverwriteDistributableLicenseMessage(string filePath)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Format_OverwriteDistributableLicense, filePath);
        }

        public static string GetMessage(MessageId messageId)
        {
            switch (messageId)
            {
                case MessageId.InvalidUpgradeExpirationDate:
                    return SR.Message_InvalidUpgradeExpirationDate;                
            }

            Debug.Assert(false);
            return null;
        }

        public static string GetCaption(CaptionId captionId)
        {
            switch (captionId)
            {
                case CaptionId.OverwriteDistributableLicense:
                    return SR.Caption_OverwriteDistributableLicense;
            }

            Debug.Assert(false);
            return null;
        }

        public static string FormatDistributableLicenseForSelfComponent(string assemblyData)
        {
            return string.Format(CultureInfo.InvariantCulture, SR.Format_GetDistributableLicense_SelfComponent, assemblyData);
        }
    }
}
