using System;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Win32;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using System.Reflection;

namespace DevZest.Licensing
{
    /// <summary>Provides license from registry.</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class RegistryLicenseProviderAttribute : CachedLicenseProviderAttribute
    {
        private RegistryHive _registryHive = RegistryHive.LocalMachine;
        private string _subkeyName;
        private string _valueName;

        /// <summary>Initializes a new instance of <see cref="RegistryLicenseProviderAttribute" /> class.</summary>
        /// <param name="subkeyName">The name or path of the subkey. When ends with "\", a [Major].[Minor] of assembly
        /// version will be appended. For example, if <paramref name="subkeyName"/> is "DevZest\.Net Licensing\" and
        /// assembly version is 1.0.3368.0, the subkey name "DevZest\.Net Licensing\1.0" will be used.</param>
        /// <param name="valueName">The name of the subkey value.</param>
        public RegistryLicenseProviderAttribute(string subkeyName, string valueName)
        {
            _subkeyName = subkeyName;
            _valueName = valueName;
        }

        /// <summary>Gets a value indicates the root registry subkey.</summary>
        /// <value>The root registry subkey. The default value is <see cref="RegistryHive">LocalMachine</see>.</value>
        [DefaultValue(RegistryHive.LocalMachine)]
        public RegistryHive RegistryHive
        {
            get { return _registryHive; }
            set
            {
                VerifyFrozenAccess();
                _registryHive = value;
            }
        }

        private RegistryKey RegistryKey
        {
            get
            {
                switch (_registryHive)
                {
                    case RegistryHive.ClassesRoot:
                        return Registry.ClassesRoot;
                    
                    case RegistryHive.CurrentConfig:
                        return Registry.CurrentConfig;

                    case RegistryHive.CurrentUser:
                        return Registry.CurrentUser;

                    case RegistryHive.DynData:
                        return Registry.DynData;

                    case RegistryHive.LocalMachine:
                        return Registry.LocalMachine;

                    case RegistryHive.PerformanceData:
                        return Registry.PerformanceData;

                    default:
                        System.Diagnostics.Debug.Assert(_registryHive == RegistryHive.Users);
                        return Registry.Users;
                }
            }
        }

        /// <summary>Gets the name or path of the subkey.</summary>
        /// <value>The name or path of the subkey. When ends with "\", a [Major].[Minor] of assembly version will be appended.
        /// For example, if the value is "DevZest\.Net Licensing\" and assembly version is 1.0.3368.0, the subkey name
        /// "DevZest\.Net Licensing\1.0" will be used.</value>
        public string SubkeyName
        {
            get { return _subkeyName; }
        }

        private string SubkeyNameToOpen
        {
            get
            {
                string subkeyName = SubkeyName;
                if (subkeyName.EndsWith(@"\", StringComparison.Ordinal))
                {
                    Version version = AssemblyLicense.GetAssemblyName(Assembly).Version;
                    subkeyName = subkeyName + string.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                }
                return subkeyName;
            }
        }

        /// <summary>Gets the name of the subkey value.</summary>
        /// <value>The name of the subkey value.</value>
        public string ValueName
        {
            get { return _valueName; }
        }

        /// <exclude />
        protected override LicenseProviderResult Load()
        {
            bool isGranted = false;
            try
            {
                string subKey = SubkeyNameToOpen;
                RegistryPermission permission = new RegistryPermission(PermissionState.Unrestricted);
                isGranted = SecurityManager.IsGranted(permission);
                if (isGranted)
                    permission.Assert();
                using (RegistryKey registryKey = RegistryKey.OpenSubKey(subKey))
                {
                    if (registryKey == null)
                        return LicenseProviderResult.FromErrorMessage(Messages.OpenRegistryKeyFailed);

                    string license = (string)registryKey.GetValue(ValueName, null);
                    if (string.IsNullOrEmpty(license))
                        return LicenseProviderResult.FromErrorMessage(Messages.EmptyRegistryValue);
                    else
                        return LicenseProviderResult.FromLicense(license);
                }
            }
            catch (SecurityException exception)
            {
                return LicenseProviderResult.FromErrorMessage(Messages.FormatReadRegistryFailed(exception.Message));
            }
            finally
            {
                if (isGranted)
                    CodeAccessPermission.RevertAssert();
            }
        }

        internal override string TraceString
        {
            get { return string.Format(CultureInfo.InvariantCulture, @"[RegistryLicenseProvider(Hive=""{0}"", Key=""{1}"", Value=""{2}"")]", RegistryHive, SubkeyNameToOpen, ValueName); }
        }
    }
}
