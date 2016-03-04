using System;
using System.Reflection;

namespace DevZest.Licensing
{
    /// <summary>Represents the result of license providers, either a license XML string, or
    /// an error message string, plus an addition Data object.</summary>
    public struct LicenseProviderResult
    {
        private string _license;
        private string _errorMessage;
        private object _data;

        /// <summary>Represents a <see cref="LicenseProviderResult" /> structure with its properties left uninitialized. </summary>
        public static LicenseProviderResult Empty
        {
            get { return new LicenseProviderResult(); }
        }

        /// <overloads>Initializes a new instance of <see cref="LicenseProviderResult" /> structure from license XML string.</overloads>
        /// <summary>Initializes a new instance of <see cref="LicenseProviderResult" /> from license XML string.</summary>
        /// <param name="license">The license XML string.</param>
        /// <returns>The created <see cref="LicenseProviderResult" /> structure.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="license"/> is null or empty.</exception>
        public static LicenseProviderResult FromLicense(string license)
        {
            return FromLicense(license, null);
        }

        /// <summary>Initializes a new instance of <see cref="LicenseProviderResult" /> from license XML string, with additional data.</summary>
        /// <param name="license">The license XML string.</param>
        /// <param name="data">The additional data.</param>
        /// <returns>The created <see cref="LicenseProviderResult" /> structure.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="license"/> is null or empty.</exception>
        public static LicenseProviderResult FromLicense(string license, object data)
        {
            if (string.IsNullOrEmpty(license))
                throw new ArgumentNullException("license");
            return new LicenseProviderResult(license, null, data);
        }

        /// <overloads>Initializes a new instance of <see cref="LicenseProviderResult" /> structure from error message string.</overloads>
        /// <summary>Initializes a new instance of <see cref="LicenseProviderResult" /> structure from error message string.</summary>
        /// <param name="errorMessage">The error message string.</param>
        /// <returns>The created <see cref="LicenseProviderResult" /> structure.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="errorMessage"/> is null or empty.</exception>
        public static LicenseProviderResult FromErrorMessage(string errorMessage)
        {
            return FromErrorMessage(errorMessage, null);
        }

        /// <summary>Initializes a new instance of <see cref="LicenseProviderResult" /> structure from error message string, with
        /// additional data.</summary>
        /// <param name="errorMessage">The error message string.</param>
        /// <param name="data">The additional data.</param>
        /// <returns>The created <see cref="LicenseProviderResult" /> structure.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="errorMessage"/> is null or empty.</exception>
        public static LicenseProviderResult FromErrorMessage(string errorMessage, object data)
        {
            if (string.IsNullOrEmpty(errorMessage))
                throw new ArgumentNullException("errorMessage");
            return new LicenseProviderResult(null, errorMessage, null);
        }

        /// <summary>Tests whether all properties of this <see cref="LicenseProviderResult" /> have values of <see langword="null"/>.</summary>
        public bool IsEmpty
        {
            get { return _license == null && _errorMessage == null && _data == null; }
        }

        private LicenseProviderResult(string license, string errorMessage, object data)
        {
            if (string.IsNullOrEmpty(license) && string.IsNullOrEmpty(errorMessage))
                throw new ArgumentNullException("license");

            if (!string.IsNullOrEmpty(license))
            {
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ArgumentException("Error message must be null or empty if license is not null or empty.", "errorMessage");
                _license = license;
                _errorMessage = null;
            }
            else if (string.IsNullOrEmpty(errorMessage))
                throw new ArgumentException("Error message must not be null or empty if license is null or empty.", "errorMessage");
            else
            {
                _license = null;
                _errorMessage = errorMessage;
            }
            _data = data;
        }

        /// <summary>Gets the encapsulated license XML string.</summary>
        /// <value>The encapsulated license XML string.</value>
        public string License
        {
            get { return _license; }
        }

        /// <summary>Gets the encapsulated error message.</summary>
        /// <value>The encapsulated error message.</value>
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        /// <summary>Gets the additional data.</summary>
        /// <value>The additional data.</value>
        public object Data
        {
            get { return _data; }
        }
    }
}
