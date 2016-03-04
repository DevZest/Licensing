using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DevZest.Licensing
{
    /// <summary>Represents the license that can be validated against the current user.</summary>
    /// <remarks>When publishing an <see cref="UserLicense" />, the <see cref="License.Data" /> property should be set as the string
    /// value returned by <see cref="CurrentUserData">UserLicense.CurrentUserData</see>, otherwise the validation will always fail.</remarks>
    public sealed class UserLicense : License
    {
        private bool _verified;
        private LicenseError _error;

        /// <exclude />
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override LicenseError Validate()
        {
            if (_verified)
            {
                if (CurrentUserData != Data)
                    _error = new LicenseError(Assembly, LicenseErrorReason.InvalidLicense, Messages.InvalidUserData, this);
                _verified = true;
            }

            return _error;
        }

        /// <summary>Gets the data for the current user.</summary>
        /// <value><see langword="string" /> represents the current user.</value>
        /// <remarks>
        /// <para>When publishing an <see cref="UserLicense" />, the <see cref="License.Data" /> property should be set as the string
        /// value returned by <see cref="CurrentUserData">UserLicense.CurrentUserData</see>,
        /// otherwise the validation will always fail.</para>
        /// <para>The returned data is formated as "[UserName];[SID]".</para>
        /// </remarks>
        public static string CurrentUserData
        {
            get
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                string sid = id.User.AccountDomainSid.ToString();
                return id.Name + ";" + sid;
            }
        }
    }
}
