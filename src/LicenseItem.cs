using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;

namespace DevZest.Licensing
{
    /// <summary>Represents a feature of <see cref="License" /> granted to an assembly.</summary>
    /// <remarks>
    /// <para>A <see cref="License" /> is created and signed by <see cref="LicensePublisher" /> which normally implemented as web service. The
    /// <see cref="LicenseClient" /> communicates with the <see cref="LicensePublisher" />, gets the published license and stores it somewhere
    /// that can be loaded by the <see cref="LicenseProviderAttribute">LicenseProvider</see> declared as assembly attribute. To determine whether
    /// an assembly can be granted a valid license, the license provider attributes are retrieved to provide an instance of <see cref="License" />
    /// object containing a collection of <see cref="LicenseItem" /> objects. The validation is then performed on the <see cref="License" />
    /// object and the <see cref="LicenseItem" /> object matching the specified license item name.</para>
    /// <para>Before performing the validation on the <see cref="LicenseItem" /> object, the containing <see cref="License" /> object seals it
    /// by setting its <see cref="Freezable{T}.IsFrozen" /> property to <see langword="true" />. Any attempting to modify
    /// the <see cref="LicenseItem" /> object throws an <see cref="InvalidOperationException" />. All derived classes should respect this rule by
    /// calling <see cref="Freezable{T}.VerifyFrozenAccess">VerifyFrozenAccess</see> first in all public methods and property setters that might
    /// modify the object.</para></remarks>
    public class LicenseItem : Freezable<License>
    {
        private License _license;
        private string _name;

        private bool _overrideExpirationDate;

        /// <overloads>Initializes a new instance of <see cref="LicenseItem" /> class.</overloads>
        /// <summary>Initializes a new instance of <see cref="LicenseItem" /> class.</summary>
        public LicenseItem()
        {
        }

        /// <summary>Initializes a new instance of <see cref="LicenseItem" /> class using specified name.</summary>
        /// <param name="name">The name of the license item.</param>
        public LicenseItem(string name)
            : this(name, false)
        {
        }

        /// <summary>Initializes a new instance of <see cref="LicenseItem" /> class using specified name, with a <see cref="Boolean" /> value
        /// indicates whether this <see cref="LicenseItem" /> should ignore its containing <see cref="License" /> object's 
        /// <see cref="DevZest.Licensing.License.ExpirationDate"/> property.</summary>
        /// <param name="name">The name of the license item.</param>
        /// <param name="overrideExpirationDate">A <see cref="Boolean" /> value indicates where this <see cref="LicenseItem" /> should ignore
        /// its containing <see cref="License" /> object's <see cref="DevZest.Licensing.License.Expiration"/> property. The default value is
        /// <see langword="false" />.</param>
        public LicenseItem(string name, bool overrideExpirationDate)
        {
            _name = name;
            _overrideExpirationDate = overrideExpirationDate;
        }

        /// <summary>Gets the containing <see cref="DevZest.Licensing.License" /> of this <see cref="LicenseItem" /> object.</summary>
        /// <value>The containing <see cref="DevZest.Licensing.License" />.</value>
        public License License
        {
            get { return _license; }
        }

        internal sealed override void Freeze(License freezer)
        {
            _license = freezer;
        }

        /// <exclude />
        public sealed override bool IsFrozen
        {
            get { return _license != null; }
        }

        /// <summary>Gets the name of this <see cref="LicenseItem" />.</summary>
        /// <value>The name of this <see cref="LicenseItem" />.</value>
        /// <remarks>This property is used to match the license item name specified by <see cref="O:DevZest.Licensing.LicenseManager.Validate">
        /// LicenseManager.Validate</see> or <see cref="O:DevZest.Licensing.LicenseManager.Check">LicenseManager.Check</see>.
        /// Adding a <see cref="LicenseItem" /> into a <see cref="LicenseItemCollection" /> with its <see cref="Name" /> property set to
        /// <see langword="null" /> or empty string throws <see cref="InvalidOperationException" />.</remarks>
        public string Name
        {
            get { return _name; }
            set
            {
                VerifyFrozenAccess();
                _name = value;
            }
        }

        /// <summary>Gets a value indicates whether this <see cref="LicenseItem" /> should ignore its containing 
        /// <see cref="License" /> object's <see cref="DevZest.Licensing.License.ExpirationDate"/> property.</summary>
        /// <value><see langword="true"/> if this <see cref="LicenseItem" /> should ignore its containing <see cref="License" /> object's
        /// <see cref="DevZest.Licensing.License.ExpirationDate"/> property, otherwise <see langword="false"/>. The default value is
        /// <see langword="false" />.</value>
        /// <remarks>Setting this property to <see langword="true" /> makes this <see cref="LicenseItem" /> never expired, even its containing
        /// <see cref="License" /> expired.</remarks>
        [DefaultValue(false)]
        public bool OverrideExpirationDate
        {
            get { return _overrideExpirationDate; }
            set
            {
                VerifyFrozenAccess();
                _overrideExpirationDate = value;
            }
        }

        /// <summary>Determines whether a valid license can be granted.</summary>
        /// <returns><see langword="null" /> if a valid license can be granted. Otherwise a <see cref="LicenseError" />
        /// indicates the error.</returns>
        protected internal virtual LicenseError Validate()
        {
            return null;
        }
    }
}
