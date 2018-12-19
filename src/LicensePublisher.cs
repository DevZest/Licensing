using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows.Markup;
using System.IO;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.Globalization;
using System.Web.Services;
using System.Text;
using System.Reflection;

namespace DevZest.Licensing
{
    /// <summary>Publishes signed licenses to <see cref="LicenseClient"/>.</summary>
    /// <remarks>
    /// <para><see cref="ILicensePublisher" /> interface is the service contract between <see cref="LicensePublisher" /> and
    /// <see cref="LicenseClient" />. <see cref="LicensePublisher" /> class implements <see cref="ILicensePublisher"/> interface throught
    /// the public <see cref="Publish">Publish</see> method. The service class derived from <see cref="LicensePublisher" /> can
    /// be hosted as traditional ASP.Net web service, or WCF (Windows Communication Foundation) service.</para>
    /// <para>The <see cref="LicensePublisher" /> class, together with <see cref="LicenseClient" /> class, handles license signing,
    /// data serialization/deserialization, data encryption/descryption, and exception handling. Sensitive data communicated between
    /// <see cref="LicenseClient" /> and <see cref="LicensePublisher" />, such as license key and published license, are encrypted so that no
    /// SSL is required.</para>
    /// <para><see cref="LicensePublisher" /> derived service class must override <see cref="GetLicense">GetLicense</see> and
    /// <see cref="GetPrivateKeyXml">GetPrivateKeyXml</see> methods. The overrided <see cref="GetPrivateKeyXml">GetPrivateKeyXml</see>
    /// must return the same private key used to sign the coresponding product assembly, to sign the published license.</para>
    /// </remarks>
    public abstract class LicensePublisher : ILicensePublisher
    {
        private static Dictionary<string, Rsa> s_cachedPrivateKeys = new Dictionary<string, Rsa>();

        private bool _cachePrivateKey;

        /// <overloads>Initializes a new instance of the <see cref="LicensePublisher" /> class.</overloads>
        /// <summary>Initializes a new instance of the <see cref="LicensePublisher" /> class that caches the private key.</summary>
        protected LicensePublisher()
            : this(true)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="LicensePublisher" /> class, given a value indicating whether the
        /// private key should be cached.</summary>
        /// <param name="cachePrivateKey">A <see langword="bool" /> value indicating whether the private key should be cached.</param>
        protected LicensePublisher(bool cachePrivateKey)
        {
            _cachePrivateKey = cachePrivateKey;
        }

        private Rsa GetPrivateKey(string product)
        {
            if (!_cachePrivateKey)
                return new Rsa(GetPrivateKeyXml(product));

            if (s_cachedPrivateKeys.ContainsKey(product))
                return s_cachedPrivateKeys[product];

            string xml = GetPrivateKeyXml(product);
            Rsa rsa = new Rsa(xml);
            s_cachedPrivateKeys.Add(product, rsa);
            return rsa;
        }

        /// <summary>Gets the private key XML string for the specified product.</summary>
        /// <param name="product">The specified product.</param>
        /// <returns>The XML string of the private key.</returns>
        /// <remarks>
        /// <format type="text/markdown">The XML string of the private key, which was used to sign the assembly corresponding the product name, should be returned by
        /// the derived class. If wrong private key is returned, the signed license can not be validated.
        /// 
        /// The derived class can call [PrivateKeyXmlFromSnkFile](xref:DevZest.Licensing.LicensePublisher.PrivateKeyXmlFromSnkFile*)
        /// to get private key XML string from a .snk file or stream.</format></remarks>
        protected abstract string GetPrivateKeyXml(string product);

        /// <summary>Gets the requested license.</summary>
        /// <param name="cultureInfo">The license client culture.</param>
        /// <param name="product">The product name.</param>
        /// <param name="version">The product version.</param>
        /// <param name="licenseKey">The license key.</param>
        /// <param name="category">The license category.</param>
        /// <param name="name">The user name.</param>
        /// <param name="company">The user company.</param>
        /// <param name="email">The user email address.</param>
        /// <param name="data">The data to set for the license.</param>
        /// <returns>The requested <see cref="License" /> or an error message encapsulated in a <see cref="LicensePublisherResponse" /> object.</returns>
        /// <remarks>The error message should respect the provided client culture.</remarks>
        protected abstract LicensePublisherResponse GetLicense(CultureInfo cultureInfo, string product, Version version, LicenseKey licenseKey, string category, string name, string company, string email, string data);

        /// <summary>Gets the requested license.</summary>
        /// <param name="culture">The license client culture LCID.</param>
        /// <param name="product">The product name.</param>
        /// <param name="version">The product version.</param>
        /// <param name="encryptedLicenseKey">The encrypted license key.</param>
        /// <param name="category">The license category.</param>
        /// <param name="name">The user name.</param>
        /// <param name="company">The user company.</param>
        /// <param name="email">The user email address.</param>
        /// <param name="data">The data to set for the license.</param>
        /// <returns>The response sent to license client.</returns>
        /// <remarks>This method is implemented as web service method accessible to license client. To secure the communication between
        /// <see cref="LicenseClient"/> and <see cref="LicensePublisher" />, the license key is encrypted by the public key of the assembly,
        /// and the response is encrypted by the license key. No SSL is required to host this web service.</remarks>
        [WebMethod]
        public string Publish(int culture, string product, string version, string encryptedLicenseKey, string category, string name, string company, string email, string data)
        {
            Rsa privateKey = GetPrivateKey(product);
            if (privateKey == null)
                throw new InvalidOperationException(ExceptionMessages.FormatNullPrivateKey(product));

            string responseString;
            LicenseKey licenseKey;
            try
            {
                licenseKey = LicenseKey.DecryptFromString(privateKey, encryptedLicenseKey);

                LicensePublisherResponse response = GetLicense(new CultureInfo(culture), product, new Version(version), licenseKey, category, name, company, email, data);
                License license = response.License;

                if (license == null)
                    responseString = LicenseClient.ErrorHeader + response.ErrorMessage;
                else
                    responseString = LicenseManager.SignLicense(privateKey, license);
            }
            finally
            {
                if (!_cachePrivateKey)
                    ((IDisposable)privateKey).Dispose();
            }
            
            return licenseKey.Encrypt(responseString);
        }

        /// <overloads>Gets the private key XML string from Strong Name Key (.snk) file or stream.</overloads>
        /// <summary>Gets the private key XML string from Strong Name Key (.snk) file.</summary>
        /// <param name="snkFilePath">Full path and name of the .snk file.</param>
        /// <returns>The private key XML string.</returns>
        public static string PrivateKeyXmlFromSnkFile(string snkFilePath)
        {
            return LicenseManager.PrivateKeyXmlFromSnkFile(snkFilePath);
        }

        /// <summary>Gets the private key XML string from Strong Name Key (.snk) stream.</summary>
        /// <param name="stream">The stream of .snk file.</param>
        /// <returns>The private key XML string.</returns>
        public static string PrivateKeyXmlFromSnkFile(Stream stream)
        {
            return LicenseManager.PrivateKeyXmlFromSnkFile(stream);
        }
    }
}
