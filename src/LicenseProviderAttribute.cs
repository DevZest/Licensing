using System;
using System.ComponentModel;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Globalization;
using System.Text;
using System.Security.Permissions;

namespace DevZest.Licensing
{
    /// <summary>Provides the abstract base class for all license providers.</summary>
    /// <remarks>
    /// <para>License provider is declared as assembly level attribute. It loads a <see cref="License"/> object from a specified location.</para>
    /// <para>When initialized during validation, the license provider is sealed by setting its <see cref="LicenseProviderAttribute.IsFrozen" />
    /// property to <see langword="true" />. Any attempting to modify the <see cref="LicenseProviderAttribute" />
    /// object throws an <see cref="InvalidOperationException" />. All derived classes should respect this rule by calling
    /// <see cref="LicenseProviderAttribute.VerifyFrozenAccess">VerifyFrozenAccess</see> in all public methods and property setters that
    /// might change the object.</para>
    /// </remarks>
    public abstract class LicenseProviderAttribute : Attribute
    {
        private bool _designMode;
        private bool _debug;
        private LicenseManager _manager;

        /// <summary>Gets or sets a value indicates whether this license provider is to load design time or runtime license.</summary>
        /// <value><see langword="true" /> if this license provider is to load design time license, otherwise <see langword="false" />.
        /// The default value is <see langword="false" />.</value>
        public bool DesignMode
        {
            get { return _designMode; }
            set
            {
                VerifyFrozenAccess();
                _designMode = value;
            }
        }

        private LicenseManager Manager
        {
            get { return _manager; }
        }

        /// <summary>Gets the assembly that this license provider attribute is declared.</summary>
        /// <value>The assembly that this license provider attribute is declared.</value>
        public Assembly Assembly
        {
            get { return _manager == null ? null : _manager.Assembly; }
        }

        internal void Freeze(LicenseManager manager)
        {
            System.Diagnostics.Debug.Assert(manager != null);
            _manager = manager;
        }

        /// <summary>Gets a value indicates whether this license provider is currently modifiable.</summary>
        /// <value><see langword="true" /> if this license provider is frozen and cannot be modified; otherwise <see langword="false" />.</value>
        /// <remarks>
        /// This property value is set to <see langword="true" /> before loading any <see cref="License" />.
        /// Attempting to modify a license provider when its <see cref="IsFrozen" /> property is true throws an <see cref="InvalidOperationException" />.
        /// </remarks>
        protected bool IsFrozen
        {
            get { return _manager != null; }
        }

        /// <summary>Enforces that this license provider is modifiable.</summary>
        /// <exception cref="InvalidOperationException">This license provider is not modifiable.</exception>
        protected internal void VerifyFrozenAccess()
        {
            if (IsFrozen)
                throw new InvalidOperationException(ExceptionMessages.FrozenAccess);
        }

        /// <summary>Provides the license.</summary>
        /// <returns>The result of license provider, either a <see cref="License" /> object or an error message string.</returns>
        /// <remarks><para>Derived class should never return <see langword="null"/>, otherwise an exception will be thrown. When
        /// multiple license provider attributes declared, the first returned non-null license will be used to validate, in the order
        /// of declaration.</para>
        /// </remarks>
        protected internal abstract LicenseProviderResult ProvideLicense();

        /// <summary>Resets this license provider.</summary>
        /// <remarks>License provider caches the lookup result by default. Calling this method can enforce license provider to perform
        /// a new lookup for the next validation, instead of returning the cached result.</remarks>
        protected internal abstract void Reset();

        /// <summary>Gets a value indicates whether trace is enabled</summary>
        /// <value><see langword="true" /> if trace is enabled, otherwise <see langword="false" />.</value>
        /// <remarks><para>Tracing is turned off by default. To turn on tracing for this license provider only, set its
        /// <see cref="Debug" /> property to <see langword="true" />; to turn on tracing for all license providers
        /// globally, add the following section to your application configuration file: </para>
        /// <code lang="XML">
        /// <![CDATA[
        ///   <system.diagnostics>
        ///    <switches>
        ///      <add name="DevZest.Licensing" value="true"/>
        ///    </switches>
        ///   </system.diagnostics>
        /// ]]>
        /// </code>
        /// </remarks>
        public bool IsTraceEnabled
        {
            get { return Debug || LicenseManager.IsTraceEnabled; }
        }

        /// <summary>Gets the diagnostics trace message.</summary>
        /// <param name="result">The result of this license provider.</param>
        /// <returns>The diagnostics trace message</returns>
        protected internal abstract string GetTraceMessage(LicenseProviderResult result);

        /// <summary>Gets or sets a value to turn on/off tracing.</summary>
        /// <value><see langword="true" /> to turn on tracing of this license provider, otherwise <see langword="false" />.</value>
        /// <remarks>
        /// <para>To turn on tracing for all license providers globally, add the following section to your application
        /// configuration file: </para>
        /// <code lang="XML">
        /// <![CDATA[
        ///   <system.diagnostics>
        ///    <switches>
        ///      <add name="DevZest.Licensing" value="true"/>
        ///    </switches>
        ///   </system.diagnostics>
        /// ]]>
        /// </code>
        /// </remarks>
        [DefaultValue(false)]
        public bool Debug
        {
            get { return _debug; }
            set
            {
                VerifyFrozenAccess();
                _debug = value;
            }
        }
    }
}
