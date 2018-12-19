using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Security.Cryptography;
using System.Reflection;
using System.Globalization;

namespace DevZest.Licensing
{
    /// <summary>Communicates with <see cref="LicensePublisher" /> to get published <see cref="License" />.</summary>
    /// <remarks><see cref="LicenseClient" /> is a WCF (Windows Communication Foundation) client object. For WCF client configuration,
    /// refer to WCF documentation.</remarks>
    public class LicenseClient : ClientBase<ILicensePublisher>
    {
        internal const string ErrorHeader = "Error:";

        private Rsa _publicKey;
        private Rsa PublicKey
        {
            get { return _publicKey; }
        }

        /// <overloads>Initializes a new instance of the <see cref="LicenseClient" /> class.</overloads>
        /// <summary>Initializes a new instance of the <see cref="LicenseClient" /> class using specified public key XML string
        /// and the default target endpoint from the application configuration file.</summary>
        /// <param name="publicKeyXml">The public key XML string.</param>
        /// <remarks>The <see href="xref:DevZest.Licensing.LicenseClient.PublicKeyXmlFromAssembly*">LicenseClient.PublicKeyXmlFromAssembly</see> method returns
        /// the public key XML string for specified assembly.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="publicKeyXml"/> is <see langword="null" /> or empty string.</exception>
        public LicenseClient(string publicKeyXml)
        {
            InternalConstruct(publicKeyXml);
        }

        /// <summary>Initializes a new instance of the <see cref="LicenseClient" /> class using specified public key XML string 
        /// and the configuration information specified in the application configuration file by endpointConfigurationName.</summary>
        /// <param name="publicKeyXml">The public key XML string.</param>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <remarks>The <see href="xref:DevZest.Licensing.LicenseClient.PublicKeyXmlFromAssembly*">LicenseClient.PublicKeyXmlFromAssembly</see> method returns
        /// the public key XML string for specified assembly.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="publicKeyXml"/> is <see langword="null" /> or empty string.</exception>
        public LicenseClient(string publicKeyXml, string endpointConfigurationName)
            : base(endpointConfigurationName)
        {
            InternalConstruct(publicKeyXml);
        }

        /// <summary>Initializes a new instance of the <see cref="LicenseClient" /> class using the specified public key XML string,
        /// binding and target address.</summary>
        /// <param name="publicKeyXml">The public key XML string.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <remarks>The <see href="xref:DevZest.Licensing.LicenseClient.PublicKeyXmlFromAssembly*">LicenseClient.PublicKeyXmlFromAssembly</see> method returns
        /// the public key XML string for specified assembly.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="publicKeyXml"/> is <see langword="null" /> or empty string.</exception>
        public LicenseClient(string publicKeyXml, Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
            InternalConstruct(publicKeyXml);
        }

        private void InternalConstruct(string publicKeyXml)
        {
            if (string.IsNullOrEmpty(publicKeyXml))
                throw new ArgumentNullException("publicKeyXml");

            _publicKey = new Rsa(publicKeyXml);
        }

        /// <overloads>Gets <see cref="License" /> from <see cref="LicensePublisher" />.</overloads>
        /// <summary>Gets <see cref="License" /> from <see cref="LicensePublisher" /> using current thread's culture.</summary>
        /// <param name="product">The product name.</param>
        /// <param name="version">The product version.</param>
        /// <param name="licenseKey">The license key.</param>
        /// <param name="category">The license category.</param>
        /// <param name="name">The user name.</param>
        /// <param name="company">The user company.</param>
        /// <param name="email">The user email address.</param>
        /// <param name="data">The data to set in the license.</param>
        /// <returns>The response from <see cref="LicensePublisher" /> which encapsulates a <see cref="License" /> object or
        /// an error message.</returns>
        public LicensePublisherResponse GetLicense(string product, Version version, LicenseKey licenseKey, string category, string name, string company, string email, string data)
        {
            return GetLicense(CultureInfo.CurrentCulture, product, version, licenseKey, category, name, company, email, data);
        }

        /// <summary>Gets <see cref="License" /> from <see cref="LicensePublisher" /> using specified culture.</summary>
        /// <param name="cultureInfo">The specified culture information.</param>
        /// <param name="product">The product name.</param>
        /// <param name="version">The product version.</param>
        /// <param name="licenseKey">The license key.</param>
        /// <param name="category">The license category.</param>
        /// <param name="name">The user name.</param>
        /// <param name="company">The user company.</param>
        /// <param name="email">The user email address.</param>
        /// <param name="data">The data to set in the license.</param>
        /// <returns>The response from <see cref="LicensePublisher" /> which encapsulates a <see cref="License" /> object or
        /// an error message.</returns>
        public LicensePublisherResponse GetLicense(CultureInfo cultureInfo, string product, Version version, LicenseKey licenseKey, string category, string name, string company, string email, string data)
        {
            string encryptedResponse = base.Channel.Publish(cultureInfo.LCID, product, version.ToString(), licenseKey.EncryptToString(PublicKey), category, name, company, email, data);
            string response = licenseKey.Decrypt(encryptedResponse);

            if (response.StartsWith(ErrorHeader, StringComparison.Ordinal))
            {
                string errorMessage = response.Substring(ErrorHeader.Length, response.Length - ErrorHeader.Length);
                return new LicensePublisherResponse(errorMessage);
            }
            else
                return new LicensePublisherResponse(LicenseManager.VerifySignedLicense(PublicKey, response));
        }

        /// <overloads>Gets public key XML string from specified assembly.</overloads>
        /// <summary>Gets public key XML string from specified assembly.</summary>
        /// <param name="assembly">The specified assembly.</param>
        /// <returns>The public key XML. <see langword="null" /> if assembly is not signed with a strong name.</returns>
        public static string PublicKeyXmlFromAssembly(Assembly assembly)
        {
            return LicenseManager.PublicKeyXmlFromAssembly(assembly);
        }

        /// <summary>Gets public key XML string from specified assembly file.</summary>
        /// <param name="assemblyPath">The full path of the assembly file.</param>
        /// <returns>The public key XML. <see langword="null" /> if assembly is not signed with a strong name.</returns>
        public static string PublicKeyXmlFromAssembly(string assemblyPath)
        {
            return LicenseManager.PublicKeyXmlFromAssembly(assemblyPath);
        }
    }
}
