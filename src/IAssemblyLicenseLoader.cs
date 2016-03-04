using System;
using System.Reflection;

namespace DevZest.Licensing
{
    /// <summary>Defines a method to load assembly license.</summary>
    public interface IAssemblyLicenseLoader
    {
        /// <summary>Loads the assembly license for specified assembly.</summary>
        /// <param name="assembly">The specified assembly.</param>
        /// <returns>The assembly license.</returns>
        string Load(Assembly assembly);
    }
}
