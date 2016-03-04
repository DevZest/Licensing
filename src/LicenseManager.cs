using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Markup;
using System.Globalization;

namespace DevZest.Licensing
{
    /// <summary>Provides static methods to get license from an assembly, determine if a valid license can be granted to an assembly,
    /// or reset license providers of an assembly.</summary>
    public sealed partial class LicenseManager
    {
        private static BooleanSwitch s_traceSwitch = new BooleanSwitch("DevZest.Licensing", "DevZest .Net Licensing Trace Switch");

        internal static bool IsTraceEnabled
        {
            get { return s_traceSwitch.Enabled; }
        }

        private static string GetTraceCategory(Assembly assembly)
        {
            return string.Format(CultureInfo.InvariantCulture, @"DevZest.Licensing(Assembly=""{0}"")", assembly);
        }

        private static void WriteTrace(Assembly assembly, string message)
        {
            Debug.Assert(IsTraceEnabled);
            Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, @"[LicenseManager(Assembly=""{0}"")] - {1}", assembly, message), GetTraceCategory(assembly));
        }

        private static void WriteTrace(LicenseProviderAttribute licenseProvider, LicenseProviderResult result)
        {
            Debug.Assert(licenseProvider.IsTraceEnabled);
            Trace.WriteLine(licenseProvider.GetTraceMessage(result), GetTraceCategory(licenseProvider.Assembly));
        }

        private static Dictionary<Assembly, LicenseManager> s_managers = new Dictionary<Assembly, LicenseManager>();
        private static Dictionary<Assembly, LicenseManager> Managers
        {
            get { return s_managers; }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static LicenseManager GetLicenseManager(Assembly assembly)
        {
            Debug.Assert(assembly != null);

            LicenseManager manager;
            if (Managers.ContainsKey(assembly))
            {
                manager = Managers[assembly];
                // Check Manager.Assembly to prevent it being changed by reflection
                if (manager.Assembly != assembly)
                    throw new InvalidOperationException();
                return manager;
            }

            manager = new LicenseManager(assembly);

            Managers.Add(assembly, manager);
            return manager;
        }

        /// <overloads>Determines whether a license can be granted.</overloads>
        /// <summary>Determines whether a runtime license can be granted for the specified license item name of executing assembly.</summary>
        /// <param name="licenseItemName">The specified license item name.</param>
        /// <remarks><para>This method throws a <see cref="LicenseException" /> when a valid <see cref="License"/> cannot be granted.
        /// The <see cref="O:DevZest.Licensing.LicenseManager.Check">LicenseManager.Check</see> method does not throw an exception.</para>
        /// <para>If the assembly is not licensed, all callers in the call stack will be checked. If any caller assembly is signed with
        /// the same strong name key, the validation will succeed. It's not neccessary to provide a <see cref="License" /> for assemblies
        /// signed with the same strong name key.</para></remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Validate(string licenseItemName)
        {
            Validate(licenseItemName, Assembly.GetCallingAssembly());
        }

        /// <overloads>Determines whether a license can be granted.</overloads>
        /// <summary>Determines whether a runtime license can be granted for the specified license item name of executing assembly.</summary>
        /// <param name="licenseItemName">The specified license item name.</param>
        /// <returns><see langword="null" /> if a valid <see cref="License" /> can be granted. Otherwise a <see cref="LicenseError" />
        /// indicates the error.</returns>
        /// <remarks><para>This method does not throw a <see cref="LicenseException" /> when a valid <see cref="License" /> cannot be granted.
        /// The <see cref="O:DevZest.Licensing.LicenseManager.Validate">LicenseManager.Validate</see> method throws exception.</para>
        /// <para>If the assembly is not licensed, all callers in the call stack will be checked. If any caller assembly is signed with
        /// the same strong name key, the validation will succeed. It's not neccessary to provide a <see cref="License" /> for assemblies
        /// signed with the same strong name key.</para></remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static LicenseError Check(string licenseItemName)
        {
            return Check(licenseItemName, Assembly.GetCallingAssembly());
        }

        /// <summary>Determines whether a runtime license can be granted for the specified license item name of the assembly containing specified type.</summary>
        /// <param name="licenseItemName">The specified license item name.</param>
        /// <param name="type">The type contained by the assembly.</param>
        /// <remarks><para>This method throws a <see cref="LicenseException" /> when a valid <see cref="License"/> cannot be granted.
        /// The <see cref="O:DevZest.Licensing.LicenseManager.Check">LicenseManager.Check</see> method does not throw an exception.</para>
        /// <para>If the assembly is not licensed, all callers in the call stack will be checked. If any caller assembly is signed with
        /// the same strong name key, the validation will succeed. It's not neccessary to provide a <see cref="License" /> for assemblies
        /// signed with the same strong name key.</para></remarks>
        public static void Validate(string licenseItemName, Type type)
        {
            Validate(licenseItemName, type.Assembly);
        }

        /// <summary>Determines whether a runtime license can be granted for the specified license item name of the assembly containing specified type.</summary>
        /// <param name="licenseItemName">The specified license item name.</param>
        /// <param name="type">The type contained by the assembly.</param>
        /// <returns><see langword="null" /> if a valid <see cref="License" /> can be granted. Otherwise a <see cref="LicenseError" />
        /// indicates the error.</returns>
        /// <remarks><para>This method does not throw a <see cref="LicenseException" /> when a valid <see cref="License" /> cannot be granted.
        /// The <see cref="O:DevZest.Licensing.LicenseManager.Validate">LicenseManager.Validate</see> method throws exception.</para>
        /// <para>If the assembly is not licensed, all callers in the call stack will be checked. If any caller assembly is signed with
        /// the same strong name key, the validation will succeed. It's not neccessary to provide a <see cref="License" /> for assemblies
        /// signed with the same strong name key.</para></remarks>
        public static LicenseError Check(string licenseItemName, Type type)
        {
            return Check(licenseItemName, type.Assembly);
        }

        /// <summary>Determines whether a runtime license can be granted for the specified license item name of the specified assembly.</summary>
        /// <param name="licenseItemName">The specified license item name.</param>
        /// <param name="assembly">The specified assembly.</param>
        /// <remarks><para>This method throws a <see cref="LicenseException" /> when a valid <see cref="License"/> cannot be granted.
        /// The <see cref="O:DevZest.Licensing.LicenseManager.Check">LicenseManager.Check</see> method does not throw an exception.</para>
        /// <para>If the assembly is not licensed, all callers in the call stack will be checked. If any caller assembly is signed with
        /// the same strong name key, the validation will succeed. It's not neccessary to provide a <see cref="License" /> for assemblies
        /// signed with the same strong name key.</para></remarks>
        public static void Validate(string licenseItemName, Assembly assembly)
        {
            Validate(licenseItemName, assembly, false);
        }

        /// <summary>Determines whether a runtime license can be granted for the specified license item name of the specified assembly.</summary>
        /// <param name="licenseItemName">The specified license item name.</param>
        /// <param name="assembly">The specified assembly.</param>
        /// <returns><see langword="null" /> if a valid <see cref="License" /> can be granted. Otherwise a <see cref="LicenseError" />
        /// indicates the error.</returns>
        /// <remarks><para>This method does not throw a <see cref="LicenseException" /> when a valid <see cref="License" /> cannot be granted.
        /// The <see cref="O:DevZest.Licensing.LicenseManager.Validate">LicenseManager.Validate</see> method throws exception.</para>
        /// <para>If the assembly is not licensed, all callers in the call stack will be checked. If any caller assembly is signed with
        /// the same strong name key, the validation will succeed. It's not neccessary to provide a <see cref="License" /> for assemblies
        /// signed with the same strong name key.</para></remarks>
        public static LicenseError Check(string licenseItemName, Assembly assembly)
        {
            return Check(licenseItemName, assembly, false);
        }

        /// <summary>Determines whether a license can be granted for the specified license item name of the executing assembly,
        /// given specified design time or runtime mode.</summary>
        /// <param name="licenseItemName">The specified license item name.</param>
        /// <param name="designMode">Specifies the design time or runtime mode. <see langword="true"/> for design time, otherwise runtime.</param>
        /// <remarks><para>This method throws a <see cref="LicenseException" /> when a valid <see cref="License"/> cannot be granted.
        /// The <see cref="O:DevZest.Licensing.LicenseManager.Check">LicenseManager.Check</see> method does not throw an exception.</para>
        /// <para>If the assembly is not licensed, all callers in the call stack will be checked. If any caller assembly is signed with
        /// the same strong name key, the validation will succeed. It's not neccessary to provide a <see cref="License" /> for assemblies
        /// signed with the same strong name key.</para></remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Validate(string licenseItemName, bool designMode)
        {
            Validate(licenseItemName, Assembly.GetCallingAssembly(), designMode);
        }

        /// <summary>Determines whether a license can be granted for the specified license item name of the executing assembly,
        /// given specified design time or runtime mode.</summary>
        /// <param name="licenseItemName">The specified license item name.</param>
        /// <param name="designMode">Specifies the design time or runtime mode. <see langword="true"/> for design time, otherwise runtime.</param>
        /// <returns><see langword="null" /> if a valid <see cref="License" /> can be granted. Otherwise a <see cref="LicenseError" />
        /// indicates the error.</returns>
        /// <remarks><para>This method does not throw a <see cref="LicenseException" /> when a valid <see cref="License" /> cannot be granted.
        /// The <see cref="O:DevZest.Licensing.LicenseManager.Validate">LicenseManager.Validate</see> method throws exception.</para>
        /// <para>If the assembly is not licensed, all callers in the call stack will be checked. If any caller assembly is signed with
        /// the same strong name key, the validation will succeed. It's not neccessary to provide a <see cref="License" /> for assemblies
        /// signed with the same strong name key.</para></remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static LicenseError Check(string licenseItemName, bool designMode)
        {
            return Check(licenseItemName, Assembly.GetCallingAssembly(), designMode);
        }

        /// <summary>Determines whether a license can be granted for the specified license item name of the assembly containing specified type,
        /// given specified design time or runtime mode.</summary>
        /// <param name="licenseItemName">The specified license item name.</param>
        /// <param name="type">The type contained by the assembly.</param>
        /// <param name="designMode">Specifies the design time or runtime mode. <see langword="true"/> for design time, otherwise runtime.</param>
        /// <remarks><para>This method throws a <see cref="LicenseException" /> when a valid <see cref="License"/> cannot be granted.
        /// The <see cref="O:DevZest.Licensing.LicenseManager.Check">LicenseManager.Check</see> method does not throw an exception.</para>
        /// <para>If the assembly is not licensed, all callers in the call stack will be checked. If any caller assembly is signed with
        /// the same strong name key, the validation will succeed. It's not neccessary to provide a <see cref="License" /> for assemblies
        /// signed with the same strong name key.</para></remarks>
        public static void Validate(string licenseItemName, Type type, bool designMode)
        {
            Validate(licenseItemName, type.Assembly, designMode);
        }

        /// <summary>Determines whether a license can be granted for the specified license item name of the assembly containing specified type,
        /// given specified design time or runtime mode.</summary>
        /// <param name="licenseItemName">The specified license item name.</param>
        /// <param name="type">The type contained by the assembly.</param>
        /// <param name="designMode">Specifies the design time or runtime mode. <see langword="true"/> for design time, otherwise runtime.</param>
        /// <returns><see langword="null" /> if a valid <see cref="License" /> can be granted. Otherwise a <see cref="LicenseError" />
        /// indicates the error.</returns>
        /// <remarks><para>This method does not throw a <see cref="LicenseException" /> when a valid <see cref="License" /> cannot be granted.
        /// The <see cref="O:DevZest.Licensing.LicenseManager.Validate">LicenseManager.Validate</see> method throws exception.</para>
        /// <para>If the assembly is not licensed, all callers in the call stack will be checked. If any caller assembly is signed with
        /// the same strong name key, the validation will succeed. It's not neccessary to provide a <see cref="License" /> for assemblies
        /// signed with the same strong name key.</para></remarks>
        public static LicenseError Check(string licenseItemName, Type type, bool designMode)
        {
            return Check(licenseItemName, type.Assembly, designMode);
        }

        /// <summary>Determines whether a license can be granted for the specified license item name of the specified assembly,
        /// given specified design time or runtime mode.</summary>
        /// <param name="licenseItemName">The specified license item name.</param>
        /// <param name="assembly">The specified assembly.</param>
        /// <param name="designMode">Specifies the design time or runtime mode. <see langword="true"/> for design time, otherwise runtime.</param>
        /// <remarks><para>This method throws a <see cref="LicenseException" /> when a valid <see cref="License"/> cannot be granted.
        /// The <see cref="O:DevZest.Licensing.LicenseManager.Check">LicenseManager.Check</see> method does not throw an exception.</para>
        /// <para>If the assembly is not licensed, all callers in the call stack will be checked. If any caller assembly is signed with
        /// the same strong name key, the validation will succeed. It's not neccessary to provide a <see cref="License" /> for assemblies
        /// signed with the same strong name key.</para></remarks>
        public static void Validate(string licenseItemName, Assembly assembly, bool designMode)
        {
            Validate(licenseItemName, assembly, designMode, true);
        }

        /// <summary>Determines whether a license can be granted for the specified license item name of the specified assembly,
        /// given specified design time or runtime mode.</summary>
        /// <param name="licenseItemName">The specified license item name.</param>
        /// <param name="assembly">The specified assembly.</param>
        /// <param name="designMode">Specifies the design time or runtime mode. <see langword="true"/> for design time, otherwise runtime.</param>
        /// <returns><see langword="null" /> if a valid <see cref="License" /> can be granted. Otherwise a <see cref="LicenseError" />
        /// indicates the error.</returns>
        /// <remarks><para>This method does not throw a <see cref="LicenseException" /> when a valid <see cref="License" /> cannot be granted.
        /// The <see cref="O:DevZest.Licensing.LicenseManager.Validate">LicenseManager.Validate</see> method throws exception.</para>
        /// <para>If the assembly is not licensed, all callers in the call stack will be checked. If any caller assembly is signed with
        /// the same strong name key, the validation will succeed. It's not neccessary to provide a <see cref="License" /> for assemblies
        /// signed with the same strong name key.</para></remarks>
        public static LicenseError Check(string licenseItemName, Assembly assembly, bool designMode)
        {
            return Validate(licenseItemName, assembly, designMode, false);
        }

        private static LicenseError Validate(string licenseItemName, Assembly assembly, bool designMode, bool throwsException)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            if (string.IsNullOrEmpty(licenseItemName))
                throw new ArgumentNullException("licenseItemName");

            LicenseError error = null;
            License license = GetLicense(assembly, designMode);
            if (license != null)
                error = license.Validate(licenseItemName);

            if (license == null || error != null)
            {
                string publicKeyToken = AssemblyLicense.GetAssemblyPublicKeyToken(assembly);

                // check the calling assembly's public key token
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null && entryAssembly != assembly)
                {
                    if (AssemblyLicense.GetAssemblyPublicKeyToken(entryAssembly) == publicKeyToken) // Entry assembly signed by the same key
                    {
                        if (IsTraceEnabled)
                            WriteTrace(assembly, Messages.FormatAssemblySignedWithSameKey(entryAssembly));
                        return null;
                    }
                }

                // check the calling assembly's public key token
                Assembly currentAssembly = Assembly.GetExecutingAssembly();

                StackTrace stackTrace = new StackTrace();
                for (int i = 1; i < stackTrace.FrameCount; i++)
                {
                    StackFrame stackFrame = stackTrace.GetFrame(i);
                    Type reflectedType = stackFrame.GetMethod().ReflectedType;
                    if (reflectedType == null)
                        continue;
                    Assembly reflectedAssembly = reflectedType.Assembly;
                    if (reflectedAssembly == assembly || reflectedAssembly == currentAssembly)
                        continue;
                    if (AssemblyLicense.GetAssemblyPublicKeyToken(reflectedAssembly) == publicKeyToken) // Calling assembly signed by the same key
                    {
                        if (IsTraceEnabled)
                            WriteTrace(assembly, Messages.FormatAssemblySignedWithSameKey(reflectedAssembly));
                        return null;
                    }
                }
            }

            if (license == null)
                error = new LicenseError(assembly, LicenseErrorReason.NullLicense, Messages.NullLicense, null);

            if (error != null && throwsException)
                throw new LicenseException(error);

            return error;
        }

        /// <overloads>Gets a license for an assembly.</overloads>
        /// <summary>Gets a license for executing assembly.</summary>
        /// <returns>A <see cref="License" /> object, or <see langword="null" /> for the assembly is not licensed.</returns>
        /// <remarks>The returned <see cref="License" /> object may not be able to be granted as a valid license. Call
        /// <see cref="O:DevZest.Licensing.LicenseManager.Check">LicenseManager.Check</see> method to determine whether a
        /// valid license can be granted.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static License GetLicense()
        {
            return GetLicense(Assembly.GetCallingAssembly(), false);
        }

        /// <summary>Gets a license for executing assembly, given specified design time or runtime mode.</summary>
        /// <param name="designMode">Specifies the design time or runtime mode. <see langword="true" /> for design time, otherwise runtime.</param>
        /// <returns>A <see cref="License" /> object, or <see langword="null" /> for the assembly is not licensed.</returns>
        /// <remarks>The returned <see cref="License" /> object may not be able to be granted as a valid license. Call
        /// <see cref="O:DevZest.Licensing.LicenseManager.Check">LicenseManager.Check</see> method to determine whether a
        /// valid license can be granted.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static License GetLicense(bool designMode)
        {
            return GetLicense(Assembly.GetCallingAssembly(), designMode);
        }

        /// <summary>Gets a license for assembly containing specified type, given specified design time or runtime mode.</summary>
        /// <param name="type">The type contained by the assembly.</param>
        /// <param name="designMode">Specifies the design time or runtime mode. <see langword="true" /> for design time, otherwise runtime.</param>
        /// <returns>A <see cref="License" /> object, or <see langword="null" /> for the assembly is not licensed.</returns>
        /// <remarks>The returned <see cref="License" /> object may not be able to be granted as a valid license. Call
        /// <see cref="O:DevZest.Licensing.LicenseManager.Check">LicenseManager.Check</see> method to determine whether a
        /// valid license can be granted.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <see langword="null"/>.</exception>
        public static License GetLicense(Type type, bool designMode)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return GetLicense(type.Assembly, designMode);
        }

        /// <summary>Gets a license for specified assembly, given specified design time or runtime mode.</summary>
        /// <param name="assembly">The specified assembly.</param>
        /// <param name="designMode">Specifies the design time or runtime mode. <see langword="true" /> for design time, otherwise runtime.</param>
        /// <returns>A <see cref="License" /> object, or <see langword="null" /> for the assembly is not licensed.</returns>
        /// <remarks>The returned <see cref="License" /> object may not be able to be granted as a valid license. Call
        /// <see cref="O:DevZest.Licensing.LicenseManager.Check">LicenseManager.Check</see> method to determine whether a
        /// valid license can be granted.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="assembly" /> is <see langword="null"/>.</exception>
        public static License GetLicense(Assembly assembly, bool designMode)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            LicenseManager manager = GetLicenseManager(assembly);
            return manager.ProvideLicense(designMode);
        }

        /// <overloads>Resets the license providers for an assembly.</overloads>
        /// <summary>Resets the license providers for the executing assembly.</summary>
        /// <remarks>License providers cache the lookup result by default. Calling this method can enforce license providers to perform
        /// a new lookup for the next validation, instead of returning the cached result.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Reset()
        {
            Reset(Assembly.GetCallingAssembly());
        }

        /// <summary>Resets the license providers for the assembly containing specified type.</summary>
        /// <param name="type">The type contained by the assembly.</param>
        /// <remarks>License providers cache the lookup result by default. Calling this method can enforce license providers to perform
        /// a new lookup for the next validation, instead of returning the cached result.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <see langword="null"/>.</exception>
        public static void Reset(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            Reset(type.Assembly);
        }

        /// <summary>Resets the license providers for specified assembly.</summary>
        /// <param name="assembly">The specified assembly.</param>
        /// <remarks>License providers cache the lookup result by default. Calling this method can enforce license providers to perform
        /// a new lookup for the next validation, instead of returning the cached result.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="assembly" /> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Reset(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            if (s_managers.ContainsKey(assembly))
            {
                LicenseManager manager = s_managers[assembly];
                Debug.Assert(manager != null);
                bool succeed = s_managers.Remove(assembly);
                Debug.Assert(succeed);
            }
        }

        // Header of license signature
        // Don't change this value, otherwise all issued license will be invalid
        private const string SignatureHeader = "Signature:";

        private static SHA1CryptoServiceProvider s_hasher;
        private static SHA1CryptoServiceProvider Hasher
        {
            get
            {
                if (s_hasher == null)
                    s_hasher = new SHA1CryptoServiceProvider();

                return s_hasher;
            }
        }

        internal static string SignLicense(Rsa rsa, License license)
        {
            string xaml = XamlWriter.Save(license);
            byte[] xamlBytes = Encoding.UTF8.GetBytes(xaml);
            byte[] signatureBytes = rsa.SignData(xamlBytes, Hasher);
            string signature = BytesToString(signatureBytes);

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(SignatureHeader + signature);
            stringBuilder.Append(xaml);
            return stringBuilder.ToString();
        }

        internal static License VerifySignedLicense(Rsa rsa, string signedLicense)
        {
            string signature, xaml;
            using (StringReader reader = new StringReader(signedLicense))
            {
                string signatureLine = reader.ReadLine();
                xaml = reader.ReadToEnd();

                if (!signatureLine.StartsWith(SignatureHeader, StringComparison.Ordinal))
                    throw new InvalidOperationException(ExceptionMessages.CanNotVerifySignedLicense);

                signature = signatureLine.Substring(SignatureHeader.Length, signatureLine.Length - SignatureHeader.Length);
            }

            byte[] xamlBytes = Encoding.UTF8.GetBytes(xaml);
            byte[] signatureBytes = BytesFromString(signature);
            if (!rsa.VerifyData(xamlBytes, Hasher, signatureBytes))
                throw new InvalidOperationException(ExceptionMessages.CanNotVerifySignedLicense);

            License license = License.LoadFromXaml(xaml);
            license.SignedString = signedLicense;
            return license;
        }

        internal static string PublicKeyXmlFromAssembly(Assembly assembly)
        {
            byte[] bytes = GetPublicKey(assembly);
            return RsaKey.GetXmlString(bytes, false);
        }

        internal static byte[] GetPublicKey(Assembly assembly)
        {
            LicensePublicKeyAttribute[] publicKeys = (LicensePublicKeyAttribute[])assembly.GetCustomAttributes(typeof(LicensePublicKeyAttribute), true);
            if (publicKeys == null || publicKeys.Length == 0)
                return assembly.GetName().GetPublicKey();
            else
                return publicKeys[0].GetPublicKey();
        }

        internal static string PublicKeyXmlFromAssembly(string assemblyPath)
        {
            return RsaKey.XmlStringFromAssembly(assemblyPath);
        }

        internal static string PrivateKeyXmlFromSnkFile(string snkFilePath)
        {
            return RsaKey.XmlStringFromSnkFile(snkFilePath);
        }

        internal static string PrivateKeyXmlFromSnkFile(Stream stream)
        {
            return RsaKey.XmlStringFromSnkFile(stream);
        }

        internal static string BytesToString(byte[] value)
        {
            return Convert.ToBase64String(value);
        }

        internal static byte[] BytesFromString(string value)
        {
            return Convert.FromBase64String(value);
        }

        private Assembly _assembly;
        private List<LicenseProviderAttribute> _designTimeProviders, _runtimeProviders;

        private LicenseManager(Assembly assembly)
        {
            Debug.Assert(assembly != null);

            _assembly = assembly;

            _designTimeProviders = new List<LicenseProviderAttribute>();
            _runtimeProviders = new List<LicenseProviderAttribute>();
            LicenseProviderAttribute[] providers = (LicenseProviderAttribute[])Assembly.GetCustomAttributes(typeof(LicenseProviderAttribute), true);
            foreach (LicenseProviderAttribute provider in providers)
            {
                provider.Freeze(this);
                if (provider.DesignMode)
                    _designTimeProviders.Add(provider);
                else
                    _runtimeProviders.Add(provider);
            }
        }

        internal Assembly Assembly
        {
            get { return _assembly; }
        }

        private License ProvideLicense(bool designMode)
        {
            List<LicenseProviderAttribute> providers = designMode ? _designTimeProviders : _runtimeProviders;

            foreach (LicenseProviderAttribute provider in providers)
            {
                // Check provider.Assembly to prevent it from being changed by reflection
                if (provider.Assembly != Assembly)
                    throw new InvalidOperationException();

                LicenseProviderResult result = provider.ProvideLicense();
                if (result.IsEmpty)
                    throw new InvalidOperationException(ExceptionMessages.EmptyLicenseProviderResult);

                if (provider.IsTraceEnabled)
                    WriteTrace(provider, result);

                License license = LoadLicense(result.License, result.Data);
                if (license == null)
                    continue;

                license.Freeze(provider);

                return license;
            }

            return null;
        }

        private License LoadLicense(string license, object providerData)
        {
            if (string.IsNullOrEmpty(license))
                return null;

            Assembly assembly = Assembly;
            string publicKeyXml = PublicKeyXmlFromAssembly(assembly);
            if (string.IsNullOrEmpty(publicKeyXml))
                throw new InvalidOperationException(ExceptionMessages.FormatNullPublicKey(assembly));
            using (Rsa publicKey = new Rsa(publicKeyXml))
            {
                License result = (License)LicenseManager.VerifySignedLicense(publicKey, license);
                result.ProviderData = providerData;
                return result;
            }
        }
    }
}
