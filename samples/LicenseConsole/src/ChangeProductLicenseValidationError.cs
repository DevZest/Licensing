using System;

namespace LicenseConsole
{
    [Flags]
    public enum ChangeProductLicenseValidationErrors
    {
        None = 0,
        LicenseKeyRequired = 1,
        UserNameRequired = 2,
        EmailAddressRequired = 4,
        AcceptLicenseAgreementRequired = 8
    }
}
