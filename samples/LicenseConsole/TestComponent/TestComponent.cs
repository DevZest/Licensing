using System;
using System.Reflection;
using DevZest.Licensing;

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.3525.0")]

// This TestComponent is expecting license from two places:
// 1. The product license from local file : %LocalAppData%DevZest\LicenseConsole\TestComponent\License.txt;
// 2. The distributable license from calling assembly.
[assembly: FileLicenseProvider(FolderOption = FolderOption.EnvironmentSpecial,
    SpecialFolder = Environment.SpecialFolder.LocalApplicationData,
    Name = @"DevZest\LicenseConsole\TestComponent\RuntimeLicense.txt")]
[assembly: AssemblyLicenseProvider(EntryAssemblyOnly = false)]

// This TestComponent has two LicenseItems: Feature1 and Feature2
[assembly: LicenseItem("Feature1")]
[assembly: LicenseItem("Feature2")]


namespace TestComponent
{
    public class MyComponent
    {
        public static void Feature1()
        {
            CheckLicense("Feature1");
        }

        public static void Feature2()
        {
            CheckLicense("Feature2");
        }

        static void CheckLicense(string licenseItem)
        {
            LicenseError error = LicenseManager.Check(licenseItem);
            if (error == null)
                Console.WriteLine(string.Format(string.Format("{0} licensed successfully.", licenseItem)));
            else
                OutputError(licenseItem, error);

        }

        private static void OutputError(string feature, LicenseError error)
        {
            Console.WriteLine(string.Format("Cannot grant a valid license to {0}, reason=\"{1}\", message=\"{2}\"",
                feature, error.Reason, error.Message));
        }
    }
}
