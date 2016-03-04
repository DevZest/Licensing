using System;
using System.IO;
using TestComponent;
using DevZest.Licensing;
using TestUnsignedApp;

[assembly: AssemblyLicenseLoader(typeof(LicenseLoader))]

namespace TestUnsignedApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Calling TestComponent.MyComponent from UNSIGNED app:");
            Console.WriteLine("----------------------------------------------------");
            MyComponent.Feature1();
            MyComponent.Feature2();
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    class LicenseLoader : IAssemblyLicenseLoader
    {
        public string Load(System.Reflection.Assembly assembly)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AssemblyLicenseProviderAttribute.GetAssemblyLicenseFileName(assembly.GetName()));
            if (!File.Exists(filePath))
                return null;

            return File.ReadAllText(filePath);
        }
    }
}
