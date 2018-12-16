---
uid: extending_dotnet_licensing
---

# Extending .Net Licensing

The default implementation of .Net Licensing can meet most of your requirements, however you still have the freedom to extend this component easily by developing custom license, license item or license provider.

## Custom License/License Item

To develop custom license, derive your class from @DevZest.Licensing.License class and override [Validate](xref:DevZest.Licensing.License#DevZest_Licensing_License_Validate) method.

To develop custom license item, derive your class from @DevZest.Licensing.LicenseItem class and override [Validate](xref:DevZest.Licensing.LicenseItem#DevZest_Licensing_LicenseItem_Validate) method.

Your class should:

Be XAML friendly. Since .Net Licensing uses XAML to serialize/deserialize license and license item object, your class must be XAML friendly. The following are some rules to writing a XAML friendly class:

- Appropriate namespaces
- Public, default constructors
- Public properties for everything you want to expose to the outside world
- Collections are usually read-only properties
- Type converters when .NET can't handle the conversion itself

For more information, see XAML documentation.

## Custom License Provider

Call [VerifyFrozenAccess](xref:DevZest.Licensing.LicenseProviderAttribute#DevZest_Licensing_LicenseProviderAttribute_VerifyFrozenAccess) method in all the public methods and property setters. Before performing the validation on the License or License Item object, the license provider seals the object and any attempt to modify the object throws an InvalidOperationException. Your derived classes should respect this rule for consistency.