using System;
using System.Diagnostics;
using System.Reflection;

namespace DevZest.Licensing
{
    /// <summary>Identifies the license validation error.</summary>
    public class LicenseError
    {
        private Assembly _assembly;
        private LicenseErrorReason _reason;
        private string _message;
        private License _license;

        /// <summary>Initializes a new instance of the <see cref="LicenseError" /> class</summary>
        /// <param name="assembly">The assembly for the license validation.</param>
        /// <param name="reason">The reason for the license validation error.</param>
        /// <param name="message">The message for the license validation error.</param>
        /// <param name="license">The <see cref="License" /> that fails the validation.</param>
        public LicenseError(Assembly assembly, LicenseErrorReason reason, string message, License license)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            _assembly = assembly;
            _reason = reason;
            _message = message;
            _license = license;
        }

        /// <summary>Gets the assembly for the license validation.</summary>
        /// <value>The assembly for the license validation.</value>
        public Assembly Assembly
        {
            get { return _assembly; }
        }

        /// <summary>Gets the reason for the license validation error.</summary>
        /// <value>The reason for the license validation error.</value>
        public LicenseErrorReason Reason
        {
            get { return _reason; }
        }

        /// <summary>Gets the message for the license validation error.</summary>
        /// <value>The message for the license validation error.</value>
        public string Message
        {
            get { return _message; }
        }

        /// <summary>Gets the <see cref="License" /> that fails the validation.</summary>
        /// <value>The <see cref="License" /> that fails the validation.</value>
        public License License
        {
            get { return _license; }
        }

        /// <summary>Gets the message for the license validation exception.</summary>
        /// <value>The message for the license validation exception.</value>
        public string ExceptionMessage
        {
            get { return Messages.FormatLicenseError(this); }
        }
    }
}
