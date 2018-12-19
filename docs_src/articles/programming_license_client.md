---
uid: programming_license_client
---

# Programming License Client

To get the license published by license publisher, you can use @DevZest.Licensing.LicenseClient class, which is a WCF (Windows Communication Foundation) client. For more information on WCF client configuration, refer to WCF documentation.

## Programming LicenseClient Class

The following sample code demonstrates how to program @DevZest.Licensing.LicenseClient class:

```csharp
private static void GetLicense(LicenseCategory licenseCategory)
{
    LicenseClient licenseClient = new LicenseClient(LicenseClient.PublicKeyXmlFromAssembly(Assembly.GetExecutingAssembly()));
    try
    {
        LicensePublisherResponse response = licenseClient.GetLicense(
            Product,
            Assembly.GetExecutingAssembly().GetName().Version,
            s_licenseKeys[(int)licenseCategory],
            licenseCategory.ToString(),
            "Test User",
            "Test Company",
            "testuser@test.com",
            MachineLicense.LocalMachineData);
        License license = response.License;
        if (license == null)
            Console.WriteLine("ERROR: " + response.ErrorMessage);
        else
        {
            File.WriteAllText(Assembly.GetExecutingAssembly().Location + ".lic", license.SignedString);
            LicenseManager.Reset();
        }

        licenseClient.Close();
    }
    catch (CommunicationException exception)
    {
        Console.WriteLine("EXCEPTION: " + exception.Message);
        licenseClient.Abort();
    }
}
```

```vb
Private Sub GetLicense(ByVal licCategory As LicenseCategory)
    Dim licClient = New LicenseClient(LicenseClient.PublicKeyXmlFromAssembly(Assembly.GetExecutingAssembly()))
    Try
        Dim response As LicensePublisherResponse = licClient.GetLicense( _
            Product, _
            Assembly.GetExecutingAssembly().GetName().Version, _
            s_licenseKeys(CType(licCategory, Integer)), _
            licCategory.ToString(), _
            "Test User", _
            "Test Company", _
            "testuser@test.com", _
            MachineLicense.LocalMachineData)
        Dim lic As License = response.License
        If lic Is Nothing Then
            Console.WriteLine("ERROR: " + response.ErrorMessage)
        Else
            File.WriteAllText(Assembly.GetExecutingAssembly().Location + ".lic", lic.SignedString)
            LicenseManager.Reset()
        End If
        licClient.Close()
    Catch ex As CommunicationException
        Console.WriteLine("EXCEPTION: " + ex.Message)
        licClient.Abort()
    End Try
End Sub
```

>[!Note]
>Use "try...catch" block instead of "using" statement. For more information, see [Close and Abort release resources safely when network connections have dropped](https://docs.microsoft.com/en-us/dotnet/framework/wcf/samples/use-close-abort-release-wcf-client-resources)

## Configure License Client

@DevZest.Licensing.LicenseClient class is a WCF client and needs to be configured to work with the license publisher. Refer to WCF documentation for more information.

### Configure for license publisher hosted as .asmx web service

The following configuration file demonstrates how to configure license client for license publisher hosted as .asmx web service (the endpoint address needs to be replaced with the actual one of course):

App.Config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.serviceModel>
    <bindings>
      <customBinding>
        <binding name="LicensePublisherSoap12">
          <textMessageEncoding messageVersion="Soap12" writeEncoding="utf-8" />
          <httpTransport />
        </binding>
      </customBinding>
    </bindings>
    <client>
      <endpoint
        address="http://localhost:16885/AsmxPublisher.asmx"
        binding="customBinding" bindingConfiguration="LicensePublisherSoap12"
        contract="DevZest.Licensing.ILicensePublisher" />      
    </client>
  </system.serviceModel>
</configuration>
```

### Configure for license publisher hosted as .svc WCF service

The following configuration file demostrates how to configure license client for license publisher hosted as .svc WCF service (the endpoint address needs to be replaced with the actual one of course):

App.Config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.serviceModel>
    <client>
      <endpoint
        address="http://localhost:16885/SvcPublisher.svc"
        binding="wsHttpBinding"
        contract="DevZest.Licensing.ILicensePublisher" />
    </client>
  </system.serviceModel>
</configuration>
```

## Integrate License Client with Your Software Product

Here is the guideline on how to integrate the license client with your software product:

- If your software product is an executable application, integrate the license client within the executable. A typical implementation is: when your application started, check the license. If there is no license granted, display the user interface to activate your product (evaluate without a license key or register with a license key), then get a license from license publisher through license client. The QuickStart Sample demonstrates how to integrate the license client this way.
- If your software product is not an executable application such as a component, develop an executable application to manage the license and distribute this application together with your product. The license client is integrated into this application and your end user can then use this application to manage licenses. The License Console Application demonstrates how to integrate the license client this way.