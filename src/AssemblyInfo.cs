using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Globalization;

namespace DevZest.Licensing
{
    /// <summary>Provides properties for getting the information about the assembly file, such as the version number, product information, public key xml, and so on.</summary>
    /// <remarks>When create a new instance of <see cref="AssemblyInfo"/> class, the assembly is loaded in another <see cref="AppDomain"/>. This <see cref="AppDomain"/> is unloaded
    /// after getting the information about the assembly.</remarks>
    public class AssemblyInfo
    {
        [Serializable]
        private class Info
        {
            public string PublicKeyXml;
            public string Product;
            public Version AssemblyVersion;
            public Version AssemblyFileVersion;
            public DateTime ReleaseDate;
            public string AssemblyData;
            public ReadOnlyCollection<KeyValuePair<string, string>> LicenseItems;
        }

        private class Reader : MarshalByRefObject
        {
            public Info Read(string assemblyFile)
            {
                Info info = new Info();

                Assembly assembly = Assembly.LoadFrom(assemblyFile);

                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes == null || attributes.Length != 1)
                    info.Product = null;
                else
                    info.Product = ((AssemblyProductAttribute)attributes[0]).Product;
                info.AssemblyVersion = assembly.GetName().Version;
                info.AssemblyFileVersion = GetAssemblyFileVersion(assembly);
                info.ReleaseDate = GetReleaseDate(info.AssemblyFileVersion);

                LicenseItemAttribute[] licenseItemAttributes = (LicenseItemAttribute[])assembly.GetCustomAttributes(typeof(LicenseItemAttribute), false);
                KeyValuePair<string, string>[] licenseItems = new KeyValuePair<string, string>[licenseItemAttributes.Length];
                for (int i = 0; i < licenseItemAttributes.Length; i++)
                {
                    licenseItems[i] = new KeyValuePair<string,string>(licenseItemAttributes[i].Name, licenseItemAttributes[i].GetDescription(assembly, CultureInfo.CurrentCulture));
                }
                info.LicenseItems = new ReadOnlyCollection<KeyValuePair<string, string>>(licenseItems);

                info.AssemblyData = AssemblyLicense.GetAssemblyData(assembly);
                info.PublicKeyXml = LicenseClient.PublicKeyXmlFromAssembly(assembly);

                return info;
            }
        }

        private string _publicKeyXml;
        private string _product;
        private Version _assemblyVersion;
        private Version _assemblyFileVersion;
        private DateTime _releaseDate;
        private string _assemblyData;
        private ReadOnlyCollection<KeyValuePair<string, string>> _licenseItems;

        /// <summary>Initializes a new instance of the <see cref="AssemblyInfo" /> class with the specified assembly file.</summary>
        /// <param name="assemblyFile">The assembly file for which to obtain the information.</param>
        public AssemblyInfo(string assemblyFile)
        {
            AppDomain domain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
            var reader = (Reader)domain.CreateInstanceAndUnwrap(typeof(Reader).Assembly.FullName, typeof(Reader).FullName);
            var info = reader.Read(assemblyFile);
            AppDomain.Unload(domain);

            _publicKeyXml = info.PublicKeyXml;
            _product = info.Product;
            _assemblyVersion = info.AssemblyVersion;
            _assemblyFileVersion = info.AssemblyFileVersion;
            _releaseDate = info.ReleaseDate;
            _assemblyData = info.AssemblyData;
            _licenseItems = info.LicenseItems;
        }

        /// <summary>Gets the product information.</summary>
        /// <value>The product information.</value>
        public string Product
        {
            get { return _product; }
        }

        /// <summary>Gets the assembly version.</summary>
        /// <value>The assembly version.</value>
        public Version AssemblyVersion
        {
            get { return _assemblyVersion; }
        }

        /// <summary>Gets the assembly file version.</summary>
        /// <value>The assembly file version.</value>
        public Version AssemblyFileVersion
        {
            get { return _assemblyFileVersion; }
        }

        /// <summary>Gets the release date.</summary>
        /// <value>The release date converted from the Build number of <see cref="AssemblyFileVersion"/> (days since Jan. 1, 2000).</value>
        public DateTime ReleaseDate
        {
            get { return _releaseDate; }
        }

        /// <summary>Gets the assembly data.</summary>
        /// <value>The assemlby data returned by <format type="text/markdown">[AssemblyLicense.GetAssemblyData](xref:DevZest.Licensing.AssemblyLicense.GetAssemblyData*)</format>.</value>
        public string AssemblyData
        {
            get { return _assemblyData; }
        }

        /// <summary>Gets the license items defined in the assembly.</summary>
        /// <value>The collection of KeyValuePair for the license items defined, the name as the key and the description as value.</value>
        public ReadOnlyCollection<KeyValuePair<string, string>> LicenseItems
        {
            get { return _licenseItems; }
        }

        /// <summary>Gets the public key XML.</summary>
        /// <value>The public key XML.</value>
        public string PublicKeyXml
        {
            get { return _publicKeyXml; }
        }

        internal static Version GetAssemblyFileVersion(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);
            Version version;
            if (attributes == null || attributes.Length == 0)
                version = AssemblyLicense.GetAssemblyName(assembly).Version;
            else
            {
                var attribute = (AssemblyFileVersionAttribute)attributes[0];
                version = new Version(attribute.Version);
            }
            return version;
        }

        /// <summary>Gets release date from the specified version number.</summary>
        /// <param name="version">The specified version number</param>
        /// <returns>The release date, calculated from the build number of the version number, since Jan 1, 2000.</returns>
        public static DateTime GetReleaseDate(Version version)
        {
            // version.Build is days since Jan. 1, 2000
            return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(version.Build);
        }

        internal static DateTime GetReleaseDate(Assembly assembly)
        {
            Version version = GetAssemblyFileVersion(assembly);
            return GetReleaseDate(version);
        }
    }
}
