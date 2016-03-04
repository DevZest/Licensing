namespace DevZest.Licensing
{
    /// <summary>Specifies the reason that fails the license validation.</summary>
    public enum LicenseErrorReason
    {
        /// <summary>The assembly is not licensed.</summary>
        NullLicense,
        /// <summary>The license is invalid.</summary>
        InvalidLicense,
        /// <summary>The license is expired.</summary>
        ExpiredLicense
    }
}
