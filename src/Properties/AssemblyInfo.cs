using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Resources;
using System.Security;
using DevZest.Licensing;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://schemas.devzest.com/licensing", "DevZest.Licensing")]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DevZest .Net Licensing")]
[assembly: AssemblyDescription("Protect .Net components/applications")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("DevZest")]
[assembly: AssemblyProduct(".Net Licensing")]
[assembly: AssemblyCopyright("Copyright © DevZest, 2009 - 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: CLSCompliant(true)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d057b901-ffe3-4431-a004-f37e3d4b7149")]

[assembly: NeutralResourcesLanguageAttribute("en")]

[assembly: AllowPartiallyTrustedCallers]

[assembly: FileLicenseProvider(SpecialFolder = Environment.SpecialFolder.LocalApplicationData, Name = @"DevZest\.Net Licensing\RuntimeLicense.txt")]
[assembly: AssemblyLicenseProvider(EntryAssemblyOnly = false)]

[assembly: LicenseItem(LicenseItems.Licensing)]
