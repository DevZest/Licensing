using System;

namespace DevZest.Licensing
{
    /// <summary>Specifies the assembly license loader.</summary>
    /// <remarks>
    /// <para>If <see cref="AssemblyLicenseLoaderAttribute"/> does not exist for the caller assembly, the assembly license is retrieved from the embedded resource of
    /// the caller assembly. The name of the embedded resource must end with the value returned by <see cref="AssemblyLicenseProviderAttribute.GetAssemblyLicenseFileName">GetAssemblyLicenseFileName</see></para>
    /// <para>If <see cref="AssemblyLicenseLoaderAttribute"/> exists for the caller assembly, a <see cref="IAssemblyLicenseLoader"/> instance of
    /// <see cref="AssemblyLicenseLoaderAttribute.LoaderType"/> will be created to retrieve the assembly license. This allows the caller assembly to customize the
    /// storage of the assembly license.</para>
    ///<para>The following example loads the assembly file from the application root directory:</para>
    /// <code lang="C#"><![CDATA[
    ///[assembly: DevZest.Licensing.EmbeddedAssemblyLicense(typeof(LicenseLoader))]
    ///
    ///class LicenseLoader : IAssemblyLicenseLoader
    ///{
    ///    public string Load(System.Reflection.Assembly assembly)
    ///    {
    ///        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AssemblyLicenseProviderAttribute.GetAssemblyLicenseFileName(assembly.GetName()));
    ///        if (!File.Exists(filePath))
    ///            return null;
    ///
    ///        return File.ReadAllText(filePath);
    ///    }
    ///}
    ///]]></code>
    ///<code lang="VB.Net"><![CDATA[
    ///<Assembly: DevZest.Licensing.EmbeddedAssemblyLicense(GetType(MyAssemblyLic.Class1))>
    ///
    ///Class LicenseLoader
    ///    Implements IAssemblyLicenseLoader
    ///    
    ///    Public Function Load(ByVal assembly As System.Reflection.Assembly) As String
    ///        Dim filePath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AssemblyLicenseProviderAttribute.GetAssemblyLicenseFileName(assembly.GetName))
    ///        If Not File.Exists(filePath) Then
    ///            Return Nothing
    ///        End If
    ///        Return File.ReadAllText(filePath)
    ///    End Function
    ///End Class
    ///]]></code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false)]
    public class AssemblyLicenseLoaderAttribute : Attribute
    {
        /// <summary>Initializes a new instance of <see cref="AssemblyLicenseLoaderAttribute"/>.</summary>
        /// <param name="loaderType">The type that implements <see cref="IAssemblyLicenseLoader"/>.</param>
        public AssemblyLicenseLoaderAttribute(Type loaderType)
        {
            if (loaderType == null)
                throw new ArgumentNullException("loaderType");
            if (!typeof(IAssemblyLicenseLoader).IsAssignableFrom(loaderType))
                throw new ArgumentException(SR.Exception_InvalidLoaderType, "loaderType");
            LoaderType = loaderType;
        }

        /// <summary>Gets the type that implements <see cref="IAssemblyLicenseLoader"/>.</summary>
        /// <value>The type that implements <see cref="IAssemblyLicenseLoader"/>.</value>
        public Type LoaderType { get; private set; }
    }
}
