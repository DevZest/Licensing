using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using System.IO;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace DevZest.Licensing
{
    /// <summary>Provides license from caller assembly.</summary>
    /// <remarks>
    /// <para>If <see cref="AssemblyLicenseLoaderAttribute"/> does not exist for the caller assembly, the assembly license is retrieved from the embedded resource of
    /// the caller assembly. The name of the embedded resource must end with the value returned by <see cref="GetAssemblyLicenseFileName">GetAssemblyLicenseFileName</see></para>
    /// <para>If <see cref="AssemblyLicenseLoaderAttribute"/> exists for the caller assembly, a <see cref="IAssemblyLicenseLoader"/> instance of
    /// <see cref="AssemblyLicenseLoaderAttribute.LoaderType"/> will be created to retrieve the assembly license. This allows the caller assembly to customize the
    /// storage of the assembly license.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false)]
    public sealed class AssemblyLicenseProviderAttribute : LicenseProviderAttribute
    {
        private bool _entryAssemblyOnly;

        private LicenseProviderResult _cachedResult;
        private Dictionary<Assembly, string> _cachedLicenses = new Dictionary<Assembly, string>();
        private Dictionary<Assembly, string> CachedLicenses
        {
            get
            {
                if (_cachedLicenses == null)
                    _cachedLicenses = new Dictionary<Assembly, string>();

                return _cachedLicenses;
            }
        }

        /// <summary>Gets a value indicates whether only the entry assembly's embedded resources are searched.</summary>
        /// <value><see langword="true" /> if only the entry assembly's embedded resources are searched, otherwise <see langword="false" />.
        /// The default value is <see langword="false" />.</value>
        [DefaultValue(false)]
        public bool EntryAssemblyOnly
        {
            get { return _entryAssemblyOnly; }
            set
            {
                VerifyFrozenAccess();
                _entryAssemblyOnly = value;
            }
        }

        /// <exclude />
        protected internal override LicenseProviderResult ProvideLicense()
        {
            if (EntryAssemblyOnly)
                return GetLicenseFromEntryAssembly();
            else
                return GetLicenseFromCallingAssembly();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private LicenseProviderResult GetLicenseFromEntryAssembly()
        {
            if (_cachedResult.IsEmpty)
                _cachedResult = LoadFromEntryAssembly();

            return _cachedResult;
        }

        private string AssemblyLicenseFileName
        {
            get { return GetAssemblyLicenseFileName(AssemblyLicense.GetAssemblyName(Assembly)); }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private LicenseProviderResult GetLicenseFromCallingAssembly()
        {
            List<StackFrame> stackFrames = IsTraceEnabled ? new List<StackFrame>() : null;
            StackTrace stackTrace = new StackTrace();
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame stackFrame = stackTrace.GetFrame(i);
                Type reflectedType = stackFrame.GetMethod().ReflectedType;
                if (reflectedType == null)
                    continue;
                Assembly callingAssembly = reflectedType.Assembly;
                if (callingAssembly == Assembly)
                    continue;

                if (IsTraceEnabled)
                    stackFrames.Add(stackFrame);
                string license;
                if (CachedLicenses.ContainsKey(callingAssembly))
                    license = CachedLicenses[callingAssembly];
                else
                {
                    license = GetLicenseFromAssembly(callingAssembly);
                    CachedLicenses.Add(callingAssembly, license);
                }

                if (license != null)
                    return LicenseProviderResult.FromLicense(license, callingAssembly);
            }

            return LicenseProviderResult.FromErrorMessage(Messages.FormatEmbeddedResourceNotFound(AssemblyLicenseFileName), stackFrames);
        }

        private LicenseProviderResult LoadFromEntryAssembly()
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
                return LicenseProviderResult.FromErrorMessage(Messages.FormatEmbeddedResourceNotFound(AssemblyLicenseFileName));

            string license = GetLicenseFromAssembly(entryAssembly);
            if (string.IsNullOrEmpty(license))
                return LicenseProviderResult.FromErrorMessage(Messages.FormatEmbeddedResourceNotFound(AssemblyLicenseFileName));
            else
                return LicenseProviderResult.FromLicense(license, entryAssembly);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private string GetLicenseFromAssembly(Assembly assembly)
        {
            System.Diagnostics.Debug.Assert(assembly != null);

            AssemblyLicenseLoaderAttribute loaderAttribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyLicenseLoaderAttribute)) as AssemblyLicenseLoaderAttribute;
            if (loaderAttribute != null)
            {
                IAssemblyLicenseLoader loader = (IAssemblyLicenseLoader)Activator.CreateInstance(loaderAttribute.LoaderType);
                return loader.Load(Assembly);
            }

            string[] resources = assembly.GetManifestResourceNames();
            if (resources == null || resources.Length == 0)
                return null;

            string suffix = AssemblyLicenseFileName;
            string resourceName = null;
            foreach (string name in resources)
            {
                if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = name;
                    break;
                }
            }

            if (resourceName == null)
                return null;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;

                using (StreamReader streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        /// <summary>Gets the file name of the caller assembly's embedded resource.</summary>
        /// <param name="assemblyName">The assembly name to be licensed.</param>
        /// <returns>The file name of the caller assembly's embedded resource.</returns>
        public static string GetAssemblyLicenseFileName(AssemblyName assemblyName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.lic", assemblyName.Name, AssemblyLicense.GetAssemblyPublicKeyToken(assemblyName));
        }

        /// <exclude />
        protected internal override void Reset()
        {
        }

        private string TraceString
        {
            get { return string.Format(CultureInfo.InvariantCulture, @"[AssemblyLicenseProvider(EntryAssemblyOnly=""{0}"")]", EntryAssemblyOnly); }
        }

        /// <exclude />
        protected internal override string GetTraceMessage(LicenseProviderResult result)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (result.License == null)
                stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", TraceString, result.ErrorMessage));
            else
                stringBuilder.Append(TraceString);

            if (result.Data != null)
            {
                if (result.Data is Assembly)
                {
                    Assembly assembly = result.Data as Assembly;
                    stringBuilder.AppendLine();
                    if (EntryAssemblyOnly)
                        stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, @"Assembly=""{0}"", EntryAssembly=""{1}""", Assembly, assembly));
                    else
                        stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, @"Assembly=""{0}"", CallingAssembly=""{1}""", Assembly, assembly));
                }
                else
                {
                    List<StackFrame> stackFrames = (List<StackFrame>)result.Data;
                    foreach (StackFrame stackFrame in stackFrames)
                    {
                        stringBuilder.AppendLine();
                        MethodBase method = stackFrame.GetMethod();
                        Assembly assembly = stackFrame.GetMethod().ReflectedType.Assembly;
                        stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, @"Method=""{0}.{1}"", Assembly=""{2}""", method.ReflectedType, method.Name, assembly));
                    }
                }
            }

            if (result.License != null)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append(result.License);
            }
            return stringBuilder.ToString();
        }
    }
}
