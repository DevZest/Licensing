---
uid: designing_license_scheme
---

# Designing License Scheme

The license scheme determines how your software product is being licensed. Since there is virtually no way to revoke a published license, plan and design your license scheme carefully in advance.

## Mapping License

You can grant different kind of licenses to the user to use your software product. For example, you might want to grant a evaluation license to the user to evaluation your software for a certain period, or grant a permanent license for paid, registered user with a valid license key. You can also grant different license for design time or runtime if you're authoring a component. How many kind of licenses you will grant to your software product, together with how you want to manage your license keys, is your business decision you have to make before releasing your software.

For each kind of license you will grant to your user, you need to map it to one @DevZest.Licensing.License object. Different kind of licenses can be of different @DevZest.Licensing.License derived class, such as @DevZest.Licensing.MachineLicense, @DevZest.Licensing.AssemblyLicense or @DevZest.Licensing.UserLicense; or of the same type with different property values. For example, .Net Licensing itself may have the following licenses mapped:

| License | Description |
| --- | --- |
| Evaluation License | @DevZest.Licensing.MachineLicense object with [Expiration](xref:DevZest.Licensing.License#DevZest_Licensing_License_Expiration) property set. This license can be obtained without a license key, valid on local machine only. |
| Paid License | @DevZest.Licensing.MachineLicense object with [Expiration](xref:DevZest.Licensing.License#DevZest_Licensing_License_Expiration) property set. This license can be only be obtained with a valid license key, valid on local machine only. |
| Distributable License | @DevZest.Licensing.AssemblyLicense object. This license can be only be obtained with a valid license key, valid for one specific caller assembly only. |

## License Provider

You have to make the decision where to store the license together with your software product. .Net Licensing provides the following license providers:

| Provider | Description |
| --- | --- |
| @DevZest.Licensing.AssemblyLicenseProviderAttribute | Provides license from caller assembly. |
| @DevZest.Licensing.FileLicenseProviderAttribute | Provides license from file. This license provider requires [FileIOPermission](https://docs.microsoft.com/en-us/dotnet/api/system.security.permissions.fileiopermission). |
| @DevZest.Licensing.IsolatedStorageFileLicenseProviderAttribute | Provides license from isolated storage file. |
| @DevZest.Licensing.RegistryLicenseProviderAttribute | Provides license from registry. This license provider requires read access of [RegistryPermission](https://docs.microsoft.com/en-us/dotnet/api/system.security.permissions.registrypermission). |

Choose the license providers based on your license types and how your software product is deployed. For example, .Net Licensing itself may have the following license providers declared:

C#:

```csharp
[assembly: RegistryLicenseProvider(@"Software\DevZest\.Net Licensing\", "RuntimeLicense")]
[assembly: AssemblyLicenseProvider(EntryAssemblyOnly=false)]
```

VB.Net:

```vb
<assembly: RegistryLicenseProvider("Software\DevZest\.Net Licensing\", "RuntimeLicense")>
<assembly: AssemblyLicenseProvider(EntryAssemblyOnly:=false)>
```

In this case, .Net Licensing will attempt to load the Product License (Evaluation or Paid License) from "RuntimeLicense" value under registry key "HKLM\Software\DevZest\.Net Licensing\X.x", where "X.x" stands for the major and minor version of DevZest.Licensing.dll. If the evaluation or developer license cannot be found, it will try to look up a Distributable License from the chain of caller assemblies.