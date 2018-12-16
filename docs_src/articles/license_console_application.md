---
uid: license_console_application
---

# License Console Application

`License Console` is an application used to manage .Net component licensing. It is extensively used by all of our component products, including .Net Licensing itself.

`License Console` is a WPF application written in C#. The source code is provided in the installed Samples zipped folder. You may modify to fit your own needs.

## The License Scheme

`License Console` supports the following types of licenses:

| License Type | License Key | Description | License Items |
| --- | --- | --- | --- |
| Free Feature License | No | Product License as @DevZest.Licensing.MachineLicense object. | Free features |
| Evaluation License | No | Product License as @DevZest.Licensing.MachineLicense object, with [Expiration](xref:DevZest.Licensing.License#DevZest_Licensing_License_ExpirationDate) property set. | All features. If corresponds a free feature, set the [OverrideExpirationDate](xref:DevZest.Licensing.LicenseItem#DevZest_Licensing_LicenseItem_OverrideExpirationDate) property to true. |
| Paid License | Yes | Product License as @DevZest.Licensing.MachineLicense object. | Determined by the provided license key. Should include all free features. |
| Distributable License | Yes/No | @DevZest.Licensing.AssemblyLicense object. | If license key provided, same as Paid License; otherwise same as Free Feature License. |

The Product License (Free Feature License, Evaluation License or Paid License) can be further divided into two sub-types: design time and runtime. It is stored as DesignTimeLicense or RuntimeLicense value of a HKLM registry key specified in the configuration file (LicenseConsole.exe.config).

## License Providers

In your assembly, you need to declare @DevZest.Licensing.RegistryLicenseProviderAttribute for Product License and @DevZest.Licensing.AssemblyLicenseProviderAttribute for Distributable License. For example, `DevZest.Licensing.dll` declares the following license providers:

```csharp
[assembly: RegistryLicenseProvider(@"Software\DevZest\.Net Licensing\", "RuntimeLicense")]
[assembly: AssemblyLicenseProvider(EntryAssemblyOnly=false)]
```

```vb
<assembly: RegistryLicenseProvider("Software\DevZest\.Net Licensing\", "RuntimeLicense")>
<assembly: AssemblyLicenseProvider(EntryAssemblyOnly:=false)>
```

The Product License will be loaded from "RuntimeLicense" value under registry key "HKLM\Software\DevZest\.Net Licensing\X.x", where "X.x" stands for the major and minor version of DevZest.Licensing.dll. The Distributable License will be loaded from the chain of caller assembly.

## LicenseItemAttribute

In your assembly, you need to declare @DevZest.Licensing.LicenseItemAttribute for `License Console` to get the full list of features (license item names and descriptions). For example, DevZest.Licensing.dll declares the following single feature:

```csharp
[assembly: LicenseItem(@".Net Licensing")]
```

```vb
<assembly: LicenseItem(".Net Licensing")>
```

You can provide localized description for each feature. This will be displayed in `License Console` main window. If no description provided, the license item name will be used.

## The Configuration File

You can configure `License Console` by modifying the configuration file `LicenseConsole.exe.config`:

```xml
<?xml version="1.0"?>
<configuration>
  <system.net>
    <defaultProxy useDefaultCredentials="true" ></defaultProxy>
  </system.net>
  <system.serviceModel>
    <bindings>
      <customBinding>
        <binding name="LicensePublisherSoap12">
          <textMessageEncoding messageVersion="Soap12" writeEncoding="utf-8"/>
          <httpTransport/>
        </binding>
      </customBinding>
    </bindings>
    <client>
      <endpoint
        address="http://www.devzest.com/LicensePublisher.asmx"
        binding="customBinding"
        bindingConfiguration="LicensePublisherSoap12"
        contract="DevZest.Licensing.ILicensePublisher"/>
    </client>
  </system.serviceModel>
  <appSettings>
    <add key="Assembly" value="DevZest.Licensing.dll"/>
    <add key="RegistryKey" value="Software\DevZest\.Net Licensing\"/>
    <add key="RegistryKeyWow6432" value="Software\Wow6432Node\DevZest\.Net Licensing\"/>
    <add key="ProductReleasesUrl" value="http://www.devzest.com/ProductReleases.aspx?Product=DotNetLicensing" />
    <add key="ProductReleasesWebPageUrl" value="http://www.devzest.com/DotNetLicensing.aspx?Show=Releases" />
    <add key="PurchaseUrl" value="http://www.devzest.com/Buy.aspx?Product=DotNetLicensing"/>
    <add key="LicenseAgreementFile" value="LicenseAgreement.rtf"/>
    <add key="LicenseEmail" value="license@devzest.com"/>
    <add key="DesignTimeFreeFeatureLicenseCategory" value=""/>
    <add key="DesignTimeEvaluationLicenseCategory" value=""/>
    <add key="DesignTimePaidLicenseCategory" value=""/>
    <add key="RuntimeFreeFeatureLicenseCategory" value=""/>
    <add key="RuntimeEvaluationLicenseCategory" value="Evaluation"/>
    <add key="RuntimePaidLicenseCategory" value="Paid"/>
    <add key="DistributableLicenseCategory" value="Distributable"/>
  </appSettings>
</configuration>
```

You need to:

- Configure the license client in the `system.serviceModel` section. For more information on how to configure the license client, see @programming_license_client.
- Configure `License Console` application in the appSettings section:

| Key | Required | Description |
| --- | --- | --- |
| Assembly | Yes | The path of your assembly. |
| RegistryKey | Yes | The registry key to store the Product License. Always use HKLM hive. When ends with "\", a `Major.Minor` version string will be appended. |
| RegistryKeyWow6432 | No | The registry key for 32 bit application to store the Product License under x64 system. Always use HKLM hive. When ends with "\", a `Major.Minor` version string will be appended. |
| ProductReleasesUrl | No | The URL of the XML file contains the release data used to check update. If not provided, checking update is disabled. (Example) |
| ProductReleasesWebPageUrl | No | The URL of the web page contains the release information. When failed reading the release data XML file, user can alternatively navigate to this web page to get the latest release. (Example) |
| PurchaseUrl | Yes | The URL that a license key can be purchased. (Example) |
| LicenseEmail | No | The email address where the license request email sent to. If value provided, you must implement the email processor to reply the request license, in the form of URL query string. Refer to the source code for required name-value pairs. |
| LicenseAgreementFile | Yes | The path of license agreement file, in RTF format. |
| DesignTimeFreeFeatureLicenseCategory | No | License category string used to communicate with the license publisher for design time Free Feature License. |
| DesignTimeEvaluationLicenseCategory | No | License category string used to communicate with the license publisher for design time Evaluation License. |
| DesignTimePaidLicenseCategory | No | License category string used to communicate with the license publisher for design time Paid License. |
| RuntimeFreeFeatureLicenseCategory | No | License category string used to communicate with the license publisher for runtime Free Feature License. |
| RuntimeEvaluationLicenseCategory | No | License category string used to communicate with the license publisher for runtime Evaluation License. |
| RuntimePaidLicenseCategory | Yes | License category string used to communicate with the license publisher for runtime Paid License. |
| DistributableLicenseCategory | Yes | License category string used to communicate with the license publisher for Distributable License. |