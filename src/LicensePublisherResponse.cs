using System;
using System.Reflection;

namespace DevZest.Licensing
{
    /// <summary>Represents the response sent from license publisher to license client.</summary>
    public class LicensePublisherResponse
    {
        private object _obj;

        /// <overloads>Initializes a new instance of <see cref="LicensePublisherResponse" /> object.</overloads>
        /// <summary>Initializes a new instance of <see cref="LicensePublisherResponse" /> object encapsulates a 
        /// <see cref="DevZest.Licensing.License" /> object.</summary>
        /// <param name="license">The encapsulated <see cref="DevZest.Licensing.License" /> object.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="license"/> is <see langword="null"/>.</exception>
        public LicensePublisherResponse(License license)
        {
            if (license == null)
                throw new ArgumentNullException("license");

            _obj = license;
        }

        /// <summary>Initializes a new instance of <see cref="LicensePublisherResponse" /> object encapsulates an error message.</summary>
        /// <param name="errorMessage">The encapsulated error message.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="errorMessage"/> is <see langword="null"/> or empty.</exception>
        public LicensePublisherResponse(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                throw new ArgumentNullException("errorMessage");

            _obj = errorMessage;
        }

        /// <summary>Gets the encapsulated <see cref="DevZest.Licensing.License"/> object.</summary>
        /// <value>The encapsulated <see cref="DevZest.Licensing.License"/> object.</value>
        public License License
        {
            get { return _obj as License; }
        }

        /// <summary>Gets the encapsulated error message.</summary>
        /// <value>The encapsulated error message.</value>
        public string ErrorMessage
        {
            get { return _obj as string; }
        }
    }
}
