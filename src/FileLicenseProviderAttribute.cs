using System;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using System.Globalization;

namespace DevZest.Licensing
{
    /// <summary>Provides license from local file system.</summary>
    /// <remarks>This license provider requires <see cref="FileIOPermission" />.
    /// If your application or component is target partial trust environment without <see cref="FileIOPermission" />, use
    /// <see cref="IsolatedStorageFileLicenseProviderAttribute" /> instead.</remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class FileLicenseProviderAttribute : CachedLicenseProviderAttribute
    {
        private FolderOption _folderOption = FolderOption.Assembly;
        private Environment.SpecialFolder _specialFolder = Environment.SpecialFolder.LocalApplicationData;
        private string _name = ".lic";

        /// <overloads>Initializes a new instance of <see cref="FileLicenseProviderAttribute" /> class.</overloads>
        /// <summary>Initializes a new instance of <see cref="FileLicenseProviderAttribute" /> class.</summary>
        public FileLicenseProviderAttribute()
        {
        }

        /// <summary>Gets or sets a value used to retrieve the directory path of the license file.</summary>
        /// <value>The value used to retrieve the directory path of the license file. The default value is <see cref="DevZest.Licensing.FolderOption.Assembly"/>.</value>
        [DefaultValue(FolderOption.Assembly)]
        public FolderOption FolderOption
        {
            get { return _folderOption; }
            set
            {
                VerifyFrozenAccess();
                _folderOption = value;
            }
        }

        /// <summary>Gets or sets a value used to retrieve directory path to system special folders.</summary>
        /// <value>A value used to retrieve directory path to system special folders. The default value is
        /// <see cref="Environment.SpecialFolder.LocalApplicationData" />. This value takes effect only when <see cref="FolderOption"/> property
        /// is <see cref="DevZest.Licensing.FolderOption.EnvironmentSpecial" />.</value>
        [DefaultValue(Environment.SpecialFolder.LocalApplicationData)]
        public Environment.SpecialFolder SpecialFolder
        {
            get { return _specialFolder; }
            set
            {
                VerifyFrozenAccess();
                _specialFolder = value;
            }
        }

        /// <summary>Gets or sets a value indicates the license file name.</summary>
        /// <value>A value indicates the license file name. The default value is ".lic".</value>
        /// <remarks>When value of <see cref="FolderOption"/> property is <see cref="F:DevZest.Licensing.FolderOption.Assembly" /> and the value
        /// starts with ".", a combination of the assembly name and this value will be used. For example, for assembly TestApp.exe,
        /// if the value of <see cref="FolderOption"/> property is <see cref="F:DevZest.Licensing.FolderOption.Assembly" /> (the default) and
        /// the value of <see cref="Name" /> property is ".lic" (the default), file name "TestApp.exe.lic" will be used.</remarks>
        public string Name
        {
            get { return _name; }
            set
            {
                VerifyFrozenAccess();
                _name = value;
            }
        }

        private string FullPath
        {
            get
            {
                if (FolderOption == FolderOption.Absolute)
                    return Name;
                else if (FolderOption == FolderOption.Assembly)
                    return FileNameFromAssembly(Assembly, Name);
                else
                {
                    System.Diagnostics.Debug.Assert(FolderOption == FolderOption.EnvironmentSpecial);
                    return Path.Combine(Environment.GetFolderPath(SpecialFolder), Name);
                }
            }
        }

        private static string FileNameFromAssembly(Assembly assembly, string name)
        {
            if (name.StartsWith(".", StringComparison.Ordinal))
                return assembly.Location + name;
            else
                return Path.Combine(Path.GetDirectoryName(assembly.Location), name);
        }

        /// <exclude />
        protected override LicenseProviderResult Load()
        {
            try
            {
                if (!File.Exists(FullPath))
                    return LicenseProviderResult.FromErrorMessage(Messages.FileNotFound);

                string license = File.ReadAllText(FullPath);
                if (string.IsNullOrEmpty(license))
                    return LicenseProviderResult.FromErrorMessage(Messages.EmptyFile);
                else
                    return LicenseProviderResult.FromLicense(license);
            }
            catch (IOException exception)
            {
                return LicenseProviderResult.FromErrorMessage(Messages.FormatReadFileFailed(exception.Message));
            }
            catch (UnauthorizedAccessException exception)
            {
                return LicenseProviderResult.FromErrorMessage(Messages.FormatReadFileFailed(exception.Message));
            }
            catch (SecurityException exception)
            {
                return LicenseProviderResult.FromErrorMessage(Messages.FormatReadFileFailed(exception.Message));
            }
        }

        internal override string TraceString
        {
            get { return string.Format(CultureInfo.InvariantCulture, @"[FileLicenseProvider(FolderOption=""{0}"", Name=""{1}"", SpecialFolder=""{2}"")]", FolderOption, Name, SpecialFolder); }
        }
    }
}
