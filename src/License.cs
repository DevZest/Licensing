using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Windows.Markup;
using System.Globalization;
using System.Security.Permissions;
using System.Runtime.CompilerServices;

namespace DevZest.Licensing
{
    /// <summary>Provides the abstract base class for all licenses. A license is granted to an assembly signed with a strong name.</summary>
    /// <remarks>
    /// <para>A <see cref="License" /> is created and signed by <see cref="LicensePublisher" /> which normally implemented as web service. The
    /// <see cref="LicenseClient" /> communicates with the <see cref="LicensePublisher" />, gets the published license and stores it somewhere
    /// that can be loaded by the <see cref="LicenseProviderAttribute">LicenseProvider</see> declared as assembly attribute. To determine whether
    /// an assembly can be granted a valid license, the license provider attributes are retrieved to provide an instance of <see cref="License" />
    /// object containing a collection of <see cref="LicenseItem" /> objects. The validation is then performed on the <see cref="License" />
    /// object and the <see cref="LicenseItem" /> object matching the specified license item name.</para>
    /// <para>Before performing the validation on the <see cref="LicenseItem" /> object, the license provider seals the <see cref="License" />
    /// object by setting its <see cref="Freezable{T}.IsFrozen" /> property to <see langword="true" />. Any attempting to modify the
    /// <see cref="License" /> object throws an <see cref="InvalidOperationException" />. All derived classes should respect this rule by
    /// calling <see cref="Freezable{T}.VerifyFrozenAccess">VerifyFrozenAccess</see> in all public methods and property setters that might
    /// modify the object.</para></remarks>
    [ContentProperty("Items")]
    public abstract class License : Freezable<LicenseProviderAttribute>
    {
        private readonly static DateTime MaxUtcDate = new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        private LicenseProviderAttribute _provider;
        private object _providerData;
        private string _signedString = string.Empty;
        private string _id = string.Empty;
        private string _category;
        private string _product = string.Empty;
        private string _company = string.Empty;
        private string _userName = string.Empty;
        private string _userCompany = string.Empty;
        private string _data;
        private LicenseItemCollection _items;

        private DateTime _expirationDate = MaxUtcDate;
        private DateTime _upgradeExpirationDate = MaxUtcDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="License" /> class
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected License()
        {
            // check stack trace for re-entrance
            StackTrace stackTrace = new StackTrace();
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                StackFrame stackFrame = stackTrace.GetFrame(i);
                MethodBase method = stackFrame.GetMethod();
                Type reflectedType = stackFrame.GetMethod().ReflectedType;
                if (reflectedType == typeof(License) && method.Name == ".ctor") // Re-entry caused by XamlReader detected
                    return;
            }

            LicenseManager.Validate(LicenseItems.Licensing);
        }

        /// <summary>Gets the assembly this license is granted to.</summary>
        /// <value>The assembly this license is granted to.</value>
        protected Assembly Assembly
        {
            get { return _provider == null ? null : _provider.Assembly; }
        }

        /// <summary>Gets the license provider which provides this license. License providers are declared as assembly level attributes.</summary>
        /// <value>The license provider which provides this license.</value>
        protected LicenseProviderAttribute Provider
        {
            get { return _provider; }
        }

        internal sealed override void Freeze(LicenseProviderAttribute provider)
        {
            Debug.Assert(provider != null);
            _provider = provider;
        }

        /// <exclude />
        public sealed override bool IsFrozen
        {
            get { return _provider != null; }
        }

        /// <summary>Gets the data set by the license provider.</summary>
        /// <value>The data set by license provider.</value>
        /// <remarks>The license provider may provide additional data for validation. For example, the assembly license provider sets this
        /// property to the calling assembly object.</remarks>
        public object ProviderData
        {
            get { return _providerData; }
            internal set
            {
                VerifyFrozenAccess();
                _providerData = value;
            }
        }

        /// <summary>Gets or sets the XAML string that can be converted from/to this <see cref="License" /> object, signed by the assembly private key.</summary>
        /// <value>The XAML string that can be converted from/to this <see cref="License" /> object, signed by the assembly private key.
        /// This property is set by the license provider.</value>
        public string SignedString
        {
            get { return _signedString; }
            internal set
            {
                VerifyFrozenAccess();
                _signedString = value;
            }
        }

        /// <summary>Gets or sets the license ID information.</summary>
        /// <value>A string containing the license ID information.</value>
        /// <remarks>This value is set by license publisher to identify this license. It does not participate the validateion process.</remarks>
        public string Id
        {
            get { return _id; }
            set
            {
                VerifyFrozenAccess();
                _id = value;
            }
        }

        /// <summary>Gets or sets the license category information.</summary>
        /// <value>A string containing the license category information.</value>
        /// <remarks>The license publisher uses this category value to publish different kind of licenses. This value does not
        /// participate the validation process.</remarks>
        public string Category
        {
            get { return _category; }
            set
            {
                VerifyFrozenAccess();
                _category = value;
            }
        }

        /// <summary>Gets or sets product name information.</summary>
        /// <value>A string containing the product name.</value>
        /// <remarks>The validation will compare this property with the <see cref="AssemblyProductAttribute.Product"/> property of the
        /// assembly level <see cref="AssemblyProductAttribute" />.</remarks>
        public string Product
        {
            get { return _product; }
            set
            {
                VerifyFrozenAccess();
                _product = value;
            }
        }

        /// <summary>Gets or sets product company information.</summary>
        /// <value>A string containing the product company name.</value>
        public string Company
        {
            get { return _company; }
            set
            {
                VerifyFrozenAccess();
                _company = value;
            }
        }

        /// <summary>Gets a collection of <see cref="LicenseItem" /> objects.</summary>
        /// <value>A <see cref="LicenseItemCollection" /></value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LicenseItemCollection Items
        {
            get
            {
                if (_items == null)
                    _items = new LicenseItemCollection(this);
                return _items;
            }
        }

        /// <summary>Gets the <see cref="LicenseItem" /> object for specified name.</summary>
        /// <param name="name">The name of the <see cref="LicenseItem"/> object.</param>
        /// <value>The <see cref="LicenseItem" /> object.</value>
        public LicenseItem this[string name]
        {
            get { return Items[name]; }
        }

        /// <summary>Gets or sets the user name information.</summary>
        /// <value>A string containing the user name information.</value>
        public string UserName
        {
            get { return _userName; }
            set
            {
                VerifyFrozenAccess();
                _userName = value;
            }
        }

        /// <summary>Gets or sets the user company information.</summary>
        /// <value>A string containing the user company information.</value>
        public string UserCompany
        {
            get { return _userCompany; }
            set
            {
                VerifyFrozenAccess();
                _userCompany = value;
            }
        }

        private static string DateToString(DateTime value)
        {
            Debug.Assert(value.Kind == DateTimeKind.Utc);

            if (value == MaxUtcDate)
                return string.Empty;

            return value.Year.ToString("0000", CultureInfo.InvariantCulture) + "/"
                + value.Month.ToString("00", CultureInfo.InvariantCulture) + "/"
                + value.Day.ToString("00", CultureInfo.InvariantCulture);
        }

        private static DateTime StringToDate(string s)
        {
            if (string.IsNullOrEmpty(s))
                return MaxUtcDate;

            int year = int.Parse(s.Substring(0, 4), CultureInfo.InvariantCulture);
            int month = int.Parse(s.Substring(5, 2), CultureInfo.InvariantCulture);
            int day = int.Parse(s.Substring(8, 2), CultureInfo.InvariantCulture);

            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        private static DateTime GetUtcDate(DateTime date)
        {
            if (date.Kind == DateTimeKind.Local)
                date = date.ToUniversalTime();

            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>Gets or sets the expiration date, as string, of this license.</summary>
        /// <value>A string containing the expiration date of this license, in the format of "yyyy/mm/dd".</value>
        /// <remarks>The value will be converted to UTC date.</remarks>
        [DefaultValue("")]
        public string Expiration
        {
            get { return DateToString(_expirationDate); }
            set
            {
                VerifyFrozenAccess();
                _expirationDate = StringToDate(value);
            }
        }

        /// <summary>Gets the expiration date of this license.</summary>
        /// <value>The UTC date of the license expiration.</value>
        public DateTime ExpirationDate
        {
            get { return _expirationDate; }
        }

        /// <summary>Sets the expiration date of this license.</summary>
        /// <param name="date">The date of the license expiration.</param>
        /// <remarks>The provided date will be converted to UTC date.</remarks>
        public void SetExpirationDate(DateTime date)
        {
            VerifyFrozenAccess();
            _expirationDate = GetUtcDate(date);
        }

        /// <summary>Gets a value indicating whether this license is expired.</summary>
        /// <value><see langword="true"/> if this license is expired, otherwise <see langword="false"/>.</value>
        public bool IsExpired
        {
            get { return DateTime.UtcNow > ExpirationDate; }
        }

        /// <summary>Gets or sets the upgrade expiration date, as string, of this license.</summary>
        /// <value>A string containing the upgrade expiration date of this license, in the format of "yyyy/mm/dd".</value>
        /// <remarks>When validating, the release date of the assembly will be compared with the upgrade exipration date of the license.
        /// The assembly version Build number specifies the release date as number of days since 2001/01/01.</remarks>
        [DefaultValue("")]
        public string UpgradeExpiration
        {
            get { return DateToString(_upgradeExpirationDate); }
            set
            {
                VerifyFrozenAccess();
                _upgradeExpirationDate = StringToDate(value);
            }
        }

        /// <summary>Gets the upgrade expiration date of this license.</summary>
        /// <value>The UTC date of the license upgrade expiration.</value>
        /// <remarks>When validating, the release date of the assembly will be compared with the upgrade exipration date of the license.
        /// The assembly version Build number specifies the release date as number of days since 2001/01/01.</remarks>
        public DateTime UpgradeExpirationDate
        {
            get { return _upgradeExpirationDate; }
        }

        /// <summary>Sets the upgrade expiration date of this license.</summary>
        /// <param name="date">The date of the license upgrade expiration.</param>
        /// <remarks>The provided date will be converted to UTC date.</remarks>
        public void SetUpgradeExpirationDate(DateTime date)
        {
            VerifyFrozenAccess();
            _upgradeExpirationDate = GetUtcDate(date);
        }

        /// <summary>Gets or sets the data that the validation is performed against.</summary>
        /// <value>A string containing the data that the validation is performed against.</value>
        public string Data
        {
            get { return _data; }
            set
            {
                VerifyFrozenAccess();
                _data = value;
            }
        }

        internal LicenseError Validate(string licenseItemName)
        {
            if (!IsFrozen)
                throw new InvalidOperationException(ExceptionMessages.LicenseMustBeFrozenBeforeValidate);

            LicenseItem licenseItem = this[licenseItemName];
            if (licenseItem == null)
                return new LicenseError(Assembly, LicenseErrorReason.InvalidLicense, Messages.FormatNoMatchingLicenseItem(licenseItemName), this);

            LicenseError error = ValidateProduct();
            if (error != null)
                return error;

            // Check ExpirationDate
            if (IsExpired && !licenseItem.OverrideExpirationDate)
                return new LicenseError(Assembly, LicenseErrorReason.ExpiredLicense, Messages.FormatExpiredLicense(ExpirationDate), this);

            // Check UpgradeExpirationDate
            DateTime releaseDate = AssemblyInfo.GetReleaseDate(Assembly);
            if (releaseDate > UpgradeExpirationDate)
                return new LicenseError(Assembly, LicenseErrorReason.InvalidLicense, Messages.FormatUpgradeExpired(UpgradeExpirationDate, releaseDate), this);

            error = Validate();
            if (error != null)
                return error;

            return licenseItem.Validate();
        }

        private LicenseError ValidateProduct()
        {
            // Check if Product property and AssemblyProductAttribute.Product matches
            Assembly assembly = Assembly;
            AssemblyProductAttribute productAttribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
            if (productAttribute != null && productAttribute.Product != Product)
                return new LicenseError(Assembly, LicenseErrorReason.InvalidLicense, Messages.FormatInvalidProductName(Product), this);
            return null;
        }

        /// <summary>Determines whether valid license can be granted.</summary>
        /// <returns><see langword="null" /> if a valid license can be granted. Otherwise a <see cref="LicenseError" />
        /// indicates the error.</returns>
        protected abstract LicenseError Validate();

        /// <summary>Loads the license from XAML string.</summary>
        /// <param name="xaml">The XAML string, returned by license publisher without signature.</param>
        /// <returns>The <see cref="License" /> object.</returns>
        /// <remarks>This method provides <see cref="License" /> object for analysis purpose only, such as in License Console application.
        /// Validating against this license throws an <see cref="InvalidOperationException" />. </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static License LoadFromXaml(string xaml)
        {
            if (string.IsNullOrEmpty(xaml))
                throw new ArgumentNullException("xaml");

            using (StringReader stringReader = new StringReader(xaml))
            {
                using (XmlReader xmlReader = XmlReader.Create(stringReader))
                {
                    try
                    {
                        return (License)XamlReader.Load(xmlReader);
                    }
                    catch (XamlParseException xamlParseException)
                    {
                        Exception exception = xamlParseException;
                        while (exception.InnerException != null)
                            exception = exception.InnerException;
                        LicenseException licenseException = exception as LicenseException;
                        if (licenseException != null)
                            throw licenseException;
                        else
                            throw;
                    }
                }
            }
        }
    }
}
