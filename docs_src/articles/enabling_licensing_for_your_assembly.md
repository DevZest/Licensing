---
uid: enabling_licensing_for_your_assembly
---

# Enabling Licensing for Your Assembly

The following needs to be done to enable licensing for your assembly:

## Sign Your Assembly

Your assembly must be signed with a strong name before enabling licensing. For more information on how to sign an assembly, refer to the documentation of .Net Framework SDK.

## Apply LicensePublicKeyAttribute (Partial Trust Only)

[FileIOPermission](https://docs.microsoft.com/en-us/dotnet/api/system.security.permissions.fileiopermission) is required to get the public key of the assembly. If you plan to distribute your assembly running under partial trust, you need to declare the @DevZest.Licensing.LicensePublicKeyAttribute for your assembly:

```csharp
[assembly: LicensePublicKey("0024000004800000940000000602000000240000525341310004000001000100ed58bddcb7bb199ed08c99bd83f732f26d49db4be3ea11c03a0c01bc0774bdcf5bbd3f00fd853f761598dd28489d9849a27e9eb901bb227d2c88b6644bd8e1b1453d021ea6b724995bdc5f839a608a5aa98f2ba6c602d25eaed7147e8046db369ad5ff0847423d926526176ff43902ee012d98f7010a5987448342107eb632b8")]
```

```vb
<assembly: LicensePublicKey("0024000004800000940000000602000000240000525341310004000001000100ed58bddcb7bb199ed08c99bd83f732f26d49db4be3ea11c03a0c01bc0774bdcf5bbd3f00fd853f761598dd28489d9849a27e9eb901bb227d2c88b6644bd8e1b1453d021ea6b724995bdc5f839a608a5aa98f2ba6c602d25eaed7147e8046db369ad5ff0847423d926526176ff43902ee012d98f7010a5987448342107eb632b8")>
```

Use the command line `sn.exe` utility to get the public key string from an signed assembly:

```powershell
sn -Tp assemblyFile
```

## Apply LicenseProviderAttribute

You must apply one or more @DevZest.Licensing.LicenseProviderAttribute to specify where the license can be retrieved for your assembly. What attributes to be used should be determined when you [Designing License Scheme](xref:designing_license_scheme). For example, .Net Licensing itself may have the following license providers declared:

```csharp
[assembly: RegistryLicenseProvider(@"Software\DevZest\.Net Licensing\", "RuntimeLicense")]
[assembly: AssemblyLicenseProvider(EntryAssemblyOnly=false)]
```

```vb
<assembly: RegistryLicenseProvider("Software\DevZest\.Net Licensing\", "RuntimeLicense")>
<assembly: AssemblyLicenseProvider(EntryAssemblyOnly:=false)>
```

.Net Licensing will attempt to load the evaluation or developer license from "RuntimeLicense" value under registry key "HKLM\Software\DevZest\.Net Licensing\X.x", where "X.x" stands for the major and minor version of DevZest.Licensing.dll. If the evaluation or developer license can not be found, it will try to look up a distributable license from the chain of caller assembly.

## Call LicenseManager Method

You can then call [LicenseManager.Validate](xref:DevZest.Licensing.LicenseManager.Validate*) or [LicenseManager.Check](xref:DevZest.Licensing.LicenseManager.Check*) to determine whether a valid license can be granted for your assembly:

```csharp
LicenseManager.Validate("Your License Item Name");
```

```vb
LicenseManager.Validate("Your License Item Name")
```

The [LicenseManager.Validate](xref:DevZest.Licensing.LicenseManager.Validate*) method throws a @DevZest.Licensing.LicenseException when a valid license cannot be granted; the [LicenseManager.Check](xref:DevZest.Licensing.LicenseManager.Check*) method does not.