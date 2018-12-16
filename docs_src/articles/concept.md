# Concept

## License
A license is a @DevZest.Licensing.License derived object, which is granted to a .Net assembly signed with a strong name. A license can be serialized/deserialized to/from a string. Below is a sample of license string:
```xml
Signature:TGX4PR3t+ZXM+qICfOMmwhgpkQPinddZo7in7MbERWxXugyzOVTRJjpyVbOp5baVWb8CB0Ix7QvNV400VxBvnKltjzqxDJad2XMt9mU8KSblpS4HAkFahEgRhU5y+0mSKi+UoUZ6p0r75PzAIOjymYsRyLHREW+gaAKdgl5g+jc=
<MachineLicense xmlns="http://schemas.devzest.com/licensing" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" Id="YA5HT6MEZE" Category="Evaluation" Product=".Net Licensing" Company="DevZest" UserName="Test User" UserCompany="Test Company" Expiration="2009/06/11" Data="AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAArWgShxVtF0+jdBrFooc0ugQAAAACAAAAAAADZgAAqAAAABAAAACtlTdlY6xg3aOu8dNrGrVbAAAAAASAAACgAAAAEAAAAIxzWfZNBIvX3sq7aUdhneQYAAAAymm1VhZo/KsPQPbX6rxsCRsDkDapoQciFAAAAOFeGq7Wq8n5a3+7Aiat50Caa+wm">
    <LicenseItem Name=".Net Licensing" />
</MachineLicense>
```
The first line is the signature of the license. The rest is the XAML text of the @DevZest.Licensing.License derived object. The signature is signed with the same key used to sign the assembly, ensuring the license text is from trusted source.
> [!WARNING]
> Only assembly signed with strong name can be granted a license. An assembly without a strong name cannot be validated by the licensing system and will throw an exception during validation.

A license contains at lease one @DevZest.Licensing.LicenseItem. A license item is a feature of the assembly, identified by a name as string. License item name can not be null, empty or duplicated.

License is copy protected. The [License.Data](xref:DevZest.Licensing.License#DevZest_Licensing_License_Data) property contains data can only be validated under a valid environment. Copying a license from the valid environment to another will invalid the license. .Net Licensing provides the following types of license implementations:

| License Type | Description |
|---|---|
| @DevZest.Licensing.MachineLicense | The [License.Data](xref:DevZest.Licensing.License#DevZest_Licensing_License_Data) property is set as the string value returned by [LocalMachineData](xref:DevZest.Licensing.MachineLicense#DevZest_Licensing_MachineLicense_LocalMachineData). [LocalMachineData](xref:DevZest.Licensing.MachineLicense#DevZest_Licensing_MachineLicense_LocalMachineData) returns a string of encrypted known GUID using DPAPI. The validation is to decrypt the [License.Data](xref:DevZest.Licensing.License#DevZest_Licensing_License_Data) property, compare it with the original known GUID. |
| @DevZest.Licensing.AssemblyLicense | The [License.Data](xref:DevZest.Licensing.License#DevZest_Licensing_License_Data) property is set as the assembly identity string value returned by [GetAssemblyData](xref:DevZest.Licensing.AssemblyLicense#DevZest_Licensing_AssemblyLicense_GetAssemblyData_System_String_). The validation is to compare the caller assembly's identity string with this string. |
| @DevZest.Licensing.UserLicense | The [License.Data](xref:DevZest.Licensing.License#DevZest_Licensing_License_Data) property is set as the string value returned by [CurrentUserData](xref:DevZest.Licensing.UserLicense#DevZest_Licensing_UserLicense_CurrentUserData). [CurrentUserData](xref:DevZest.Licensing.UserLicense#DevZest_Licensing_UserLicense_CurrentUserData) returns the string contains the name and SID of current user. The validation is to compare the current user's name and SID with this string. |

> [!WARNING]
> @DevZest.Licensing.MachineLicense requires [DataProtectionPermission](https://docs.microsoft.com/en-us/dotnet/api/system.security.permissions.dataprotectionpermission). If your application or component is targeting partial trust environment without [DataProtectionPermission](https://docs.microsoft.com/en-us/dotnet/api/system.security.permissions.dataprotectionpermission), use @DevZest.Licensing.UserLicense instead.

## License Publisher and License Client
License is created, signed and published by @DevZest.Licensing.LicensePublisher. Since license publisher must hold the private key to sign the license, it is normally implemented as a separate web service instead of as part of the licensed application or component.

@DevZest.Licensing.LicenseClient communicates with license publisher. The [GetLicense](xref:DevZest.Licensing.LicenseClient#DevZest_Licensing_LicenseClient_GetLicense_System_Globalization_CultureInfo_System_String_System_Version_DevZest_Licensing_LicenseKey_System_String_System_String_System_String_System_String_System_String_) method sends license creation request to license publisher, and gets the created and signed license.

License client requests the license by providing a @DevZest.Licensing.LicenseKey. Typically a license key is generated and stored in the web server, sent to the user when the purchase completed. The license publisher verifies the provided license key, returns corresponding license, or returns an error when verification failed.

When sending the request to license publisher, the license client encrypts the license key using the public key. The license publisher then uses the private key to decrypt the license key, and uses this license key to encrypt the response sent back to license client. Since the communication is encrypted, no SSL is required. This saves cost for web hosting and improves the performance.

## License Providers
The published license must be stored together with the licensed assembly. To specify where the license can be located, the assembly author declares license provider attributes for the assembly. .Net Licensing provides the following license providers:

| Provider | Description |
| --- | --- |
| @DevZest.Licensing.AssemblyLicenseProviderAttribute | Provides license from embedded resource of caller assembly. |
| @DevZest.Licensing.FileLicenseProviderAttribute | Provides license from file. This license provider requires [FileIOPermission](https://docs.microsoft.com/en-us/dotnet/api/system.security.permissions.fileiopermission). |
| @DevZest.Licensing.IsolatedStorageFileLicenseProviderAttribute | Provides license from isolated storage file. |
| @DevZest.Licensing.RegistryLicenseProviderAttribute | Provides license from registry. This license provider requires read access of [RegistryPermission](https://docs.microsoft.com/en-us/dotnet/api/system.security.permissions.registrypermission). |

If the license string can be loaded, the license provider verifies the signature and returns a @DevZest.Licensing.License object. An exception will throw if verification failed.

## Validation
To validate the assembly, call [LicenseManager.Validate](xref:DevZest.Licensing.LicenseManager#DevZest_Licensing_LicenseManager_Validate_System_String_) or [LicenseManager.Check](xref:DevZest.Licensing.LicenseManager#DevZest_Licensing_LicenseManager_Check_System_String_) in the assembly. The validation will:
1. Retrieve the license provider attributes of the assembly, in the order of declaration, get the first non null @DevZest.Licensing.License object. If all license providers returns null, the validation fails.
2. Validate this @DevZest.Licensing.License object and its coresponding @DevZest.Licensing.LicenseItem object. The validation will fail if the @DevZest.Licensing.LicenseItem object with specified name does not exist.

## Summary
A @DevZest.Licensing.License is created and signed by @DevZest.Licensing.LicensePublisher which normally implemented as web service. The @DevZest.Licensing.LicenseClient communicates with the @DevZest.Licensing.LicensePublisher, gets the published license and stores it somewhere that can be loaded by the @DevZest.Licensing.LicenseProviderAttribute declared as assembly attribute. To determine whether an assembly can be granted a valid license, the license provider attributes are retrieved to provide an instance of @DevZest.Licensing.License object containing a collection of @DevZest.Licensing.LicenseItem objects. The validation is then performed on the @DevZest.Licensing.License object and the @DevZest.Licensing.LicenseItem object matching the specified license item name:

![image](../images/concept_summary.jpg)

1. The License Client sends request to License Publisher.
2. The License Publisher publishes the license string.
3. The published license string is stored in a place accessible to the assembly.
4. The license is loaded by license provider and validated at runtime.