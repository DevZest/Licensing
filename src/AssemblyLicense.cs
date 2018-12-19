using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Security.Policy;

namespace DevZest.Licensing
{
    /// <summary>Represents the license that can be validated against the calling assembly.</summary>
    /// <remarks><para>When publishing an <see cref="AssemblyLicense" />, the <see cref="License.Data" /> property should be set as the string
    /// value returned by <see href="xref:DevZest.Licensing.AssemblyLicense.GetAssemblyData*">AssemblyLicense.GetAssemblyData</see>, otherwise the validation will always fail.</para>
    /// <para>The <see cref="AssemblyLicense" /> is loaded by <see cref="AssemblyLicenseProviderAttribute" />.</para></remarks>
    [Obfuscation(Exclude = true, StripAfterObfuscation = true)]
    public sealed class AssemblyLicense : License
    {
        private bool _verified;
        private LicenseError _error;

        /// <exclude />
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override LicenseError Validate()
        {
            if (!_verified)
            {
                Assembly assembly = ProviderData as Assembly;
                Debug.Assert(assembly != null);
                string data = GetAssemblyData(assembly);
                if (string.IsNullOrEmpty(Data) || data != Data)
                    _error = new LicenseError(Assembly, LicenseErrorReason.InvalidLicense, Messages.InvalidAssemblyData, this);

                _verified = true;
            }

            return _error;
        }

        /// <overloads>Gets the data for the specified calling assembly.</overloads>
        /// <summary>Gets the data for the specified calling assembly.</summary>
        /// <param name="assembly">The specified calling assembly.</param>
        /// <returns><see langword="string" /> represents the calling assembly.</returns>
        /// <remarks>
        /// <para>When publishing an <see cref="AssemblyLicense" />, the <see cref="License.Data" /> property should be set as the string
        /// value returned by <see href="xref:DevZest.Licensing.AssemblyLicense.GetAssemblyData*">AssemblyLicense.GetAssemblyData</see>,
        /// otherwise the validation will always fail.</para>
        /// <para>The returned data is formated as "[AssemblyName],[PublicKeyToken]" if the assembly is signed with a strong name,
        /// or "[AssemblyName]:[AssemblyHash]" if the assembly is not signed. When assembly is not signed, every compile may result
        /// in a different assembly hash, therefore a different AssemblyLicense is required.</para>
        /// </remarks>
        public static string GetAssemblyData(Assembly assembly)
        {
            if (IsAssemblySigned(assembly))
                return string.Format(CultureInfo.InvariantCulture, "{0},{1}", GetAssemblyName(assembly).Name, GetAssemblyPublicKeyToken(assembly));
            else
                return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", GetAssemblyName(assembly).Name, GetAssemblyHash(assembly));
        }

        /// <summary>Gets the data for the specified calling assembly file.</summary>
        /// <param name="assemblyFile">The path of the calling assembly file.</param>
        /// <returns><see langword="string" /> represents the calling assembly file.</returns>
        /// <remarks>
        /// <para>When publishing an <see cref="AssemblyLicense" />, the <see cref="License.Data" /> property should be set as the string
        /// value returned by <see href="xref:DevZest.Licensing.AssemblyLicense.GetAssemblyData*">AssemblyLicense.GetAssemblyData</see>,
        /// otherwise the validation will always fail.</para>
        /// <para>The returned data is formated as "[AssemblyName],[PublicKeyToken]" if the assembly is signed with a strong name,
        /// or "[AssemblyName]:[AssemblyHash]" if the assembly is not signed. When assembly is not signed, every compile may result
        /// in a different assembly hash, therefore a different AssemblyLicense is required.</para>
        /// </remarks>
        public static string GetAssemblyData(string assemblyFile)
        {
            AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
            if (IsAssemblySigned(assemblyName))
                return string.Format(CultureInfo.InvariantCulture, "{0},{1}", assemblyName.Name, GetAssemblyPublicKeyToken(assemblyName));
            else
                return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", assemblyName.Name, GetAssemblyHash(assemblyName));
        }

        internal static string GetAssemblyPublicKeyToken(AssemblyName assemblyName)
        {
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");

            Byte[] publicKeyToken = assemblyName.GetPublicKeyToken();

            if (publicKeyToken == null || publicKeyToken.Length == 0)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < publicKeyToken.Length; i++)
                stringBuilder.Append(publicKeyToken[i].ToString("x", CultureInfo.InvariantCulture));
            return stringBuilder.ToString();
        }

        internal static string GetAssemblyPublicKeyToken(Assembly assembly)
        {
            return GetAssemblyPublicKeyToken(GetAssemblyName(assembly));
        }

        internal static AssemblyName GetAssemblyName(Assembly assembly)
        {
            return new AssemblyName(assembly.FullName);
        }

        internal static bool IsAssemblySigned(Assembly assembly)
        {
            return !string.IsNullOrEmpty(GetAssemblyPublicKeyToken(assembly));
        }

        internal static bool IsAssemblySigned(AssemblyName assembly)
        {
            return !string.IsNullOrEmpty(GetAssemblyPublicKeyToken(assembly));
        }

        private static string GetAssemblyHash(AssemblyName assemblyName)
        {
            Assembly assembly = Assembly.ReflectionOnlyLoadFrom(assemblyName.CodeBase);
            return GetAssemblyHash(assembly);
        }

        private static string GetAssemblyHash(Assembly assembly)
        {
            foreach (object obj in GetEnumerable(assembly.Evidence.GetHostEnumerator()))
            {
                Hash hash = obj as Hash;
                if (hash != null)
                    return LicenseManager.BytesToString(hash.SHA1);
            }

            return null;
        }

        private static IEnumerable GetEnumerable(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
    }
}
