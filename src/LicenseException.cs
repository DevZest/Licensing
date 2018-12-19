using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Reflection;

namespace DevZest.Licensing
{
    /// <summary>Represents the exception thrown when license validation failed.</summary>
    /// <remarks>The <format type="text/markdown">[LicenseManager.Validate](xref:DevZest.Licensing.LicenseManager.Validate*)</format> method throws 
    /// <see cref="LicenseException" /> when validation failed. This occurs when an assembly is either not licensed, or is licensed but
    /// cannot be granted a valid license.</remarks>
    [Serializable]
    public sealed class LicenseException : Exception
    {
        private LicenseError _error;

        /// <summary>Initializes a new instance of the <see cref="LicenseException" /> class with a specified <see cref="LicenseError" />.</summary>
        /// <param name="error">The specified <see cref="LicenseError"/>.</param>
        public LicenseException(LicenseError error)
            : base(error.ExceptionMessage)
        {
            if (error == null)
                throw new ArgumentNullException("error");
            _error = error;
        }

        /// <overloads>Initializes a new instance of the <see cref="LicenseException" /> class.</overloads>
        /// <summary>Initializes a new instance of the <see cref="LicenseException" /> class.</summary>
        public LicenseException()
            : base()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="LicenseException"/> class with a specified error message.</summary>
        /// <param name="message">The message that describes the error.</param>
        public LicenseException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="LicenseException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> if no inner exception is specified.</param>
        public LicenseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="LicenseException"/> class with serialized data.</summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        private LicenseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _error = (LicenseError)info.GetValue("Error", typeof(LicenseError));
        }

        /// <summary>Gets the <see cref="LicenseError" /> that causes this <see cref="LicenseException" />.</summary>
        /// <value>The <see cref="LicenseError"/> that causes this <see cref="LicenseException" />.</value>
        public LicenseError Error
        {
            get { return _error; }
        }

        /// <exclude />
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Error", _error);
            base.GetObjectData(info, context);
        }
    }
}
