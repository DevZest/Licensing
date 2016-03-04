using System;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;
using System.Resources;

namespace DevZest.Licensing
{
    /// <summary>Declares license items of an assembly.</summary>
    /// <remarks><see cref="LicenseItemAttribute" /> class declares license items of an assembly and does not participate the license
    /// publish nor validation process. The declared license items can have a localizable description so that this information can be
    /// used by utility such as License Console.</remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class LicenseItemAttribute : Attribute
    {
        private string _name;
        private string _descriptionResourceBaseName;
        private string _descriptionResourceName;

        /// <overloads>Initializes a new instance of <see cref="LicenseItemAttribute" /> class.</overloads>
        /// <summary>Initializes a new instance of <see cref="LicenseItemAttribute" /> class with specified license item name.</summary>
        /// <param name="name">The name of license item.</param>
        public LicenseItemAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            _name = name;
        }

        /// <summary>Initializes a new instance of <see cref="LicenseItemAttribute" /> class with specified license item name and localizable
        /// description.</summary>
        /// <param name="name">The name of license item.</param>
        /// <param name="descriptionResourceBaseName">The resource root name of the description.</param>
        /// <param name="descriptionResourceName">The resource name of the description.</param>
        public LicenseItemAttribute(string name, string descriptionResourceBaseName, string descriptionResourceName)
        {
            if (string.IsNullOrEmpty(descriptionResourceBaseName))
                throw new ArgumentNullException("descriptionResourceBaseName");

            if (string.IsNullOrEmpty(descriptionResourceName))
                throw new ArgumentNullException("descriptionResourceName");

            _name = name;
            _descriptionResourceBaseName = descriptionResourceBaseName;
            _descriptionResourceName = descriptionResourceName;
        }

        /// <summary>Gets the license item name.</summary>
        /// <value>The license item name.</value>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>Gets the resource root name of the description.</summary>
        /// <value>The resource root name of the description. For example,  the root name for the resource file named "MyResource.en-US.resources"
        /// is "MyResource". </value>
        public string DescriptionResourceBaseName
        {
            get { return _descriptionResourceBaseName; }
        }

        /// <summary>Gets the resource name of the description.</summary>
        /// <value>The resource name of the description.</value>
        public string DescriptionResourceName
        {
            get { return _descriptionResourceName; }
        }

        /// <summary>Gets the descritpion of the license item, using current thread's culture.</summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The description of the license item.</returns>
        public string GetDescription(Assembly assembly)
        {
            return GetDescription(assembly, CultureInfo.CurrentCulture);
        }

        /// <summary>Gets the descritpion of the license item, using specified culture.</summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="culture">The specified culture.</param>
        /// <returns>The description of the license item.</returns>
        public string GetDescription(Assembly assembly, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(_descriptionResourceBaseName))
                return Name;

            ResourceManager resourceManager = new ResourceManager(_descriptionResourceBaseName, assembly);
            return resourceManager.GetString(_descriptionResourceName, culture);
        }
    }
}
