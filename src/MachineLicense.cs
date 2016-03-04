using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Security.Cryptography;
using System.Reflection;
using System.Security.Permissions;
using System.Runtime.CompilerServices;

namespace DevZest.Licensing
{
    /// <summary>Represents the license that can be validated against the local machine.</summary>
    /// <remarks><para>When publishing an <see cref="MachineLicense" />, the <see cref="License.Data" /> property should be set as the string
    /// value returned by <see cref="LocalMachineData">MachineLicense.LocalMachineData</see>, otherwise the validation will always fail.</para>
    /// <para><see cref="MachineLicense" /> uses DPAPI to encrypt a known GUID as its <see cref="License.Data" /> property. The validation
    /// is to decrypt the <see cref="License.Data" /> property, compare it with the known GUID. It requires <see cref="DataProtectionPermission" />.
    /// If your application or component is target partial trust environment without <see cref="DataProtectionPermission" />, use
    /// <see cref="UserLicense" /> instead.</para></remarks>
    public sealed class MachineLicense : License
    {
        // {1F079CBE-96C8-46fd-8631-2C7D7E4DA8CA}
        private static readonly Guid Guid = new Guid(0x1f079cbe, 0x96c8, 0x46fd, 0x86, 0x31, 0x2c, 0x7d, 0x7e, 0x4d, 0xa8, 0xca);

        bool _verified;
        private LicenseError _error;

        /// <exclude />
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected sealed override LicenseError Validate()
        {
            if (_verified)
            {
                Byte[] encryptedBytes = LicenseManager.BytesFromString(Data);
                Byte[] bytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.LocalMachine);
                if (new Guid(bytes) != Guid)
                    _error = new LicenseError(Assembly, LicenseErrorReason.InvalidLicense, Messages.InvalidMachineData, this);
                _verified = true;
            }

            return _error;
        }

        /// <summary>Gets the data for local machine.</summary>
        /// <value>A <see cref="string" /> value containing the data for local machine.</value>
        /// <remarks><para>When publishing an <see cref="MachineLicense" />, the <see cref="License.Data" /> property should be set as the string
        /// value returned by <see cref="LocalMachineData">MachineLicense.LocalMachineData</see>, otherwise the validation will always fail.</para>
        /// <para><see cref="MachineLicense" /> uses DPAPI to encrypt a known GUID as its <see cref="License.Data" /> property. The validation
        /// is to decrypt the <see cref="License.Data" /> property, compare it with the known GUID. It requires <see cref="DataProtectionPermission" />.
        /// If your application or component is target partial trust environment without <see cref="DataProtectionPermission" />, use
        /// <see cref="UserLicense" /> instead.</para></remarks>
        public static string LocalMachineData
        {
            get
            {
                byte[] protectedDataBytes = ProtectedData.Protect(Guid.ToByteArray(), null, DataProtectionScope.LocalMachine);
                return LicenseManager.BytesToString(protectedDataBytes);
            }
        }
    }
}
