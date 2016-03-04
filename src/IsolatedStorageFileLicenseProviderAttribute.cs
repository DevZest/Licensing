using System;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Reflection;
using System.Globalization;

namespace DevZest.Licensing
{
    /// <summary>Provides license from isolated storage file.</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class IsolatedStorageFileLicenseProviderAttribute : CachedLicenseProviderAttribute
    {
        private IsolatedStorageScope _scope;
        private Type _applicationEvidenceType;
        private string _path;

        /// <summary>Initializes a new instance of <see cref="IsolatedStorageFileLicenseProviderAttribute" /> class.</summary>
        /// <param name="scope">A bitwise combination of the <see cref="IsolatedStorageScope" /> values.</param>
        /// <param name="applicationEvidenceType">An <see cref="Evidence" /> object containing the application identity.</param>
        /// <param name="path">The path of the isolated storage file.</param>
        public IsolatedStorageFileLicenseProviderAttribute(IsolatedStorageScope scope, Type applicationEvidenceType, string path)
        {
            _scope = scope;
            _applicationEvidenceType = applicationEvidenceType;
            _path = path;
        }

        /// <summary>Gets the isolation scope.</summary>
        /// <value>A bitwise combination of the <see cref="IsolatedStorageScope" /> values.</value>
        public IsolatedStorageScope Scope
        {
            get { return _scope; }
        }

        /// <summary>Gets the application identity object.</summary>
        /// <value>An <see cref="Evidence" /> object containing the application identity.</value>
        public Type ApplicationEvidenceType
        {
            get { return _applicationEvidenceType; }
        }

        /// <summary>Gets the path of the isolated storage file.</summary>
        /// <value>The path of the isolated storage file.</value>
        public string Path
        {
            get { return _path; }
        }

        /// <exclude />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        protected override LicenseProviderResult Load()
        {
            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetStore(Scope, ApplicationEvidenceType))
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(Path, FileMode.Open, FileAccess.Read, store))
                    {
                        if (stream == null)
                            return LicenseProviderResult.FromErrorMessage(Messages.FileNotFound);

                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string xaml = reader.ReadToEnd();
                            if (string.IsNullOrEmpty(xaml))
                                return LicenseProviderResult.FromErrorMessage(Messages.EmptyFile);
                            else
                                return LicenseProviderResult.FromLicense(reader.ReadToEnd());
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return LicenseProviderResult.FromErrorMessage(Messages.FileNotFound);
            }
            catch (SecurityException exception)
            {
                return LicenseProviderResult.FromErrorMessage(Messages.FormatReadFileFailed(exception.Message));
            }
            catch (IsolatedStorageException exception)
            {
                return LicenseProviderResult.FromErrorMessage(Messages.FormatReadFileFailed(exception.Message));
            }
        }

        internal override string TraceString
        {
            get { return string.Format(CultureInfo.InvariantCulture, @"[IsolatedStorageFileLicenseProvider(Scope=""{0}"", ApplicationEvidenceType=""{1}"", Path=""{2}"")]", Scope, ApplicationEvidenceType, Path); }
        }
    }
}
