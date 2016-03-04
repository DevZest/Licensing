using System;
using System.Diagnostics;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace DevZest.Licensing
{
    /// <summary>Provides cached license.</summary>
    public abstract class CachedLicenseProviderAttribute : LicenseProviderAttribute
    {
        private LicenseProviderResult _result;

        /// <exclude />
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal sealed override LicenseProviderResult ProvideLicense()
        {
            if (_result.IsEmpty)
            {
                _result = Load();
                if (_result.IsEmpty)
                    throw new InvalidOperationException(ExceptionMessages.EmptyLicenseProviderResult);
            }

            return _result;
        }

        /// <summary>Loads a <see cref="License" /> when there is no cached value.</summary>
        /// <returns>The result of loaded <see cref="License" />.</returns>
        protected abstract LicenseProviderResult Load();

        /// <exclude />
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal sealed override void Reset()
        {
            _result = new LicenseProviderResult();
        }

        internal abstract string TraceString { get; }
        
        /// <exclude />
        protected internal override string GetTraceMessage(LicenseProviderResult result)
        {
            if (result.License == null)
                return string.Format(CultureInfo.InvariantCulture, "{0} - {1}", TraceString, result.ErrorMessage);
            else
                return string.Format(CultureInfo.InvariantCulture, "{0}\n{1}", TraceString, result.License);
        }
    }
}
