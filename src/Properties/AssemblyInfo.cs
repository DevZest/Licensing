using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Resources;
using System.Security;
using DevZest.Licensing;
using System.Windows.Markup;

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.5.0.0")]
[assembly: AssemblyFileVersion("1.5.4767.0")]

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

[assembly: LicensePublicKey("0024000004800000940000000602000000240000525341310004000001000100ed58bddcb7bb19" +
            "9ed08c99bd83f732f26d49db4be3ea11c03a0c01bc0774bdcf5bbd3f00fd853f761598dd28489d" +
            "9849a27e9eb901bb227d2c88b6644bd8e1b1453d021ea6b724995bdc5f839a608a5aa98f2ba6c6" +
            "02d25eaed7147e8046db369ad5ff0847423d926526176ff43902ee012d98f7010a598744834210" +
            "7eb632b8")]
[assembly: FileLicenseProvider(SpecialFolder = Environment.SpecialFolder.LocalApplicationData, Name = @"DevZest\.Net Licensing\RuntimeLicense.txt")]
[assembly: AssemblyLicenseProvider(EntryAssemblyOnly = false)]

[assembly: LicenseItem(LicenseItems.Licensing)]
