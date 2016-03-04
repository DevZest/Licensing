using System;
using System.IO;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using DevZest.Licensing;
using System.Xml.Serialization;
using System.Xml;

namespace LicenseConsole
{
    internal class LicenseEntry
    {
        private LicenseKey _licenseKey;
        private string _name;
        private string _company;
        private string _email;
        private string _runtimeLicenseString;
        private License _runtimeLicense;
        private string _disabledRuntimeLicenseString;
        private License _disabledRuntimeLicense;

        public LicenseKey LicenseKey
        {
            get { return _licenseKey; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Company
        {
            get { return _company; }
        }

        public string Email
        {
            get { return _email; }
        }

        public License RuntimeLicense
        {
            get { return _runtimeLicense; }
        }

        public License DisabledRuntimeLicense
        {
            get { return _disabledRuntimeLicense; }
        }

        private static string Folder
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ConfigurationManager.AppSettings["LicenseEntryFolder"]); }
        }

        private static string LicenseInfoFile
        {
            get { return Path.Combine(Folder, "LicenseInfo.txt"); }
        }

        private static string DesignTimeLicenseFile
        {
            get { return Path.Combine(Folder, "DesignTimeLicense.txt"); }
        }

        private static string RuntimeLicenseFile
        {
            get { return Path.Combine(Folder, "RuntimeLicense.txt"); }
        }

        private static string DisabledRuntimeLicenseFile
        {
            get { return Path.Combine(Folder, "RuntimeLicense.Disabled.txt"); }
        }

        public static void Save(LicenseKey licenseKey, string name, string company, string email, string designtimeLicense, string runtimeLicense)
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            var info = new LicenseInfo
            {
                LicenseKey = EncryptLicenseKey(licenseKey),
                Name = name,
                Company = company,
                Email = email
            };

            XmlSerializer serializer = new XmlSerializer(info.GetType());
            StringBuilder result = new StringBuilder();
            using (var writer = XmlWriter.Create(result))
            {
                serializer.Serialize(writer, info);
            }
            File.WriteAllText(LicenseInfoFile, result.ToString());

            File.WriteAllText(DesignTimeLicenseFile, designtimeLicense);

            var runtimeLicenseFile = File.Exists(DisabledRuntimeLicenseFile) ? DisabledRuntimeLicenseFile : RuntimeLicenseFile;
            File.WriteAllText(runtimeLicenseFile, runtimeLicense);
        }

        public static LicenseEntry Load()
        {
            LicenseEntry result = new LicenseEntry();

            if (File.Exists(LicenseInfoFile))
            {
                var serializer = new XmlSerializer(typeof(LicenseInfo));
                using (var reader = XmlTextReader.Create(new StringReader(File.ReadAllText(LicenseInfoFile))))
                {
                    var info = (LicenseInfo)serializer.Deserialize(reader);
                    result._licenseKey = DecryptLicenseKey(info.LicenseKey);
                    result._name = info.Name;
                    result._company = info.Company;
                    result._email = info.Email;
                }
            }

            LoadRuntimeLicense(result);
            LoadDisabledRuntimeLicense(result);

            return result;
        }

        private static void LoadRuntimeLicense(LicenseEntry licenseEntry)
        {
            if (File.Exists(RuntimeLicenseFile))
            {
                var licenseString = File.ReadAllText(RuntimeLicenseFile);
                licenseEntry._runtimeLicenseString = licenseString;
                licenseEntry._runtimeLicense = GetLicense(licenseString);
            }
        }

        private static void LoadDisabledRuntimeLicense(LicenseEntry licenseEntry)
        {
            if (File.Exists(DisabledRuntimeLicenseFile))
            {
                var licenseString = File.ReadAllText(DisabledRuntimeLicenseFile);
                licenseEntry._disabledRuntimeLicenseString = licenseString;
                licenseEntry._disabledRuntimeLicense = GetLicense(licenseString);
            }
        }

        private static string EncryptLicenseKey(LicenseKey licenseKey)
        {
            if (licenseKey.IsEmpty)
                return string.Empty;

            byte[] bytes = Encoding.UTF8.GetBytes(licenseKey.ToString());
            byte[] protectedDataBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(protectedDataBytes);
        }

        private static LicenseKey DecryptLicenseKey(string encryptedLicenseKey)
        {
            if (string.IsNullOrEmpty(encryptedLicenseKey))
                return LicenseKey.Empty;

            byte[] protectedDataBytes = Convert.FromBase64String(encryptedLicenseKey);
            byte[] bytes = ProtectedData.Unprotect(protectedDataBytes, null, DataProtectionScope.LocalMachine);
            return new LicenseKey(Encoding.UTF8.GetString(bytes));
        }

        private static License GetLicense(string licenseString)
        {
            if (string.IsNullOrEmpty(licenseString))
                return null;

            string xaml;
            using (StringReader reader = new StringReader(licenseString))
            {
                reader.ReadLine(); // strip the signature line
                xaml = reader.ReadToEnd();
            }

            return License.LoadFromXaml(xaml);
        }

        public static void ToggleRuntimeLicense()
        {
            if (File.Exists(RuntimeLicenseFile))
                File.Move(RuntimeLicenseFile, DisabledRuntimeLicenseFile);
            else if (File.Exists(DisabledRuntimeLicenseFile))
                File.Move(DisabledRuntimeLicenseFile, RuntimeLicenseFile);
        }
    }
}
