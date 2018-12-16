---
uid: programming_license_publisher
---

# Programming License Publisher

To develop your license publisher, derive your class from the abstract @DevZest.Licensing.LicensePublisher class, and implement your licensing logic here. Normally this class is hosted as ASP.Net .asmx or .svc web service. For a step by step walk-through on how to create a license publisher, see [SamplePublisher](xref:sample_publisher).

## Implement Class Derived from LicensePublisher

The abstract class @DevZest.Licensing.LicensePublisher has two abstract methods: [GetPrivateKeyXml](xref:DevZest.Licensing.LicensePublisher#DevZest_Licensing_LicensePublisher_GetPrivateKeyXml_System_String_) and [GetLicense](xref:DevZest.Licensing.LicensePublisher#DevZest_Licensing_LicensePublisher_GetLicense_System_Globalization_CultureInfo_System_String_System_Version_DevZest_Licensing_LicenseKey_System_String_System_String_System_String_System_String_System_String_). The derived class must provide the implementation by overriding these two methods.

### Override GetPrivateKeyXml

The derived class must override the [GetPrivateKeyXml](xref:DevZest.Licensing.LicensePublisher#DevZest_Licensing_LicensePublisher_GetPrivateKeyXml_System_String_) method to provide the private key used to sign the license. The implementation must return the same private key used to sign the corresponding product assembly, otherwise the published license cannot be validated. You can embed the .snk file as embedded resource, and call PrivateKeyXmlFromSnkFile to get the private key xml:

```csharp
using System;
using System.Diagnostics;
using System.Collections.Generic;
using DevZest.Licensing;
using System.IO;
using System.Web.Services;
using System.Reflection;
using System.Globalization;

namespace SamplePublisher
{
    public class Publisher : LicensePublisher
    {
        ...
        protected override string GetPrivateKeyXml(string product)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SamplePublisher.Key.snk"))
            {
                return PrivateKeyXmlFromSnkFile(stream);
            }
        }
        ...
    }
}
```

```vb
Imports System
Imports System.Diagnostics
Imports System.Collections.Generic
Imports DevZest.Licensing
Imports System.IO
Imports System.Web.Services
Imports System.Reflection
Imports System.Globalization

<WebService(Namespace:="http://services.devzest.com/Licensing")> _
Public Class Publisher
    Inherits LicensePublisher
    ...
    Protected Overrides Function GetPrivateKeyXml(ByVal product As String) As String
        Using stream As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SamplePublisher.Key.snk")
            Return PrivateKeyXmlFromSnkFile(stream)
        End Using
    End Function
    ...
End Class
```

>[!Caution]
>The private key is the utmost secret of your licensing system. If your private key is compromised, the whole licensing system is compromised. Since your license publisher holds the private key, make sure your license publisher is hosted in a secure, trusted environment.

### Override GetLicense

The derived class must override the [GetLicense](xref:DevZest.Licensing.LicensePublisher#DevZest_Licensing_LicensePublisher_GetLicense_System_Globalization_CultureInfo_System_String_System_Version_DevZest_Licensing_LicenseKey_System_String_System_String_System_String_System_String_System_String_) method to provide the license to publish, based on the combination of provided product name, license key and license category. Since all your business logic is implemented here, this is the core of your licensing system.

```csharp
using System;
using System.Diagnostics;
using System.Collections.Generic;
using DevZest.Licensing;
using System.IO;
using System.Web.Services;
using System.Reflection;
using System.Globalization;

namespace SamplePublisher
{
    public class Publisher : LicensePublisher
    {
        ...
        protected override LicensePublisherResponse GetLicense(CultureInfo cultureInfo, string product, Version version, LicenseKey licenseKey, string category, string userName, string company, string emailAddress, string data)
        {
            ...
        }
        ...
    }
}
```

```vb
Imports System
Imports System.Diagnostics
Imports System.Collections.Generic
Imports DevZest.Licensing
Imports System.IO
Imports System.Web.Services
Imports System.Reflection
Imports System.Globalization

<WebService(Namespace:="http://services.devzest.com/Licensing")> _
Public Class Publisher
    Inherits LicensePublisher
    ...
    Protected Overrides Function GetLicense(ByVal cultureInfo As CultureInfo, ByVal product As String, ByVal version As Version, ByVal licenseKey As LicenseKey, ByVal category As String, ByVal userName As String, ByVal company As String, ByVal emailAddress As String, ByVal data As String) As LicensePublisherResponse
        ...
    End Function
    ...
End Class
```

>[!Caution]
>Since there is virtually no way to revoke a published license, test your implementation thoroughly before publishing any license to public.

## Hosting License Publisher as Web Service

The developed license publisher class is a WCF (Windows Communication Foundation) service implementing service contract @DevZest.Licensing.ILicensePublisher. Every WCF service must be hosted in a Windows process called the host process. Typically you can host your developed license publisher class as traditional ASP.Net .asmx web service or WCF .svc web service.

For more information, see WCF documentation on MSDN.

### Hosting as .asmx Web Service

To host your license publisher as .asmx web service, add a web service file (AsmxPublisher.asmx) to your ASP.Net Application:

```aspx
<%@ WebService Language="C#" CodeBehind="AsmxPublisher.asmx.cs" Class="SamplePublisher.AsmxPublisher" %>
```

```csharp
using System;
using System.Web;
using System.Web.Services;

namespace SamplePublisher
{
    [WebService(Namespace = "http://services.devzest.com/Licensing")]
    public class AsmxPublisher : Publisher
    {
    }
}
```

```aspx
<%@ WebService Language="vb" CodeBehind="AsmxPublisher.asmx.vb" Class="SamplePublisher.AsmxPublisher" %>
```

```vb
Imports System.Web.Services

<WebService(Namespace:="http://services.devzest.com/Licensing")> _
Public Class AsmxPublisher
    Inherits Publisher

End Class
```

### Hosting as .svc Web Service

To host your license publisher as .svc web service:

#### Add an .svc WCF service file (SvcPublisher.svc) to your ASP.Net application

```aspx
<%@ ServiceHost Language="C#" Debug="true" Service="SamplePublisher.SvcPublisher" CodeBehind="SvcPublisher.svc.cs" %>
```

```csharp
using System;
using System.ServiceModel;

namespace SamplePublisher
{
    public class SvcPublisher : Publisher
    {
    }
}
```

```aspx
<%@ ServiceHost Language="VB" Debug="true" Service="SamplePublisher.SvcPublisher" CodeBehind="SvcPublisher.svc.vb" %>
```

```vb
Public Class SvcPublisher
    Inherits Publisher
End Class
```

#### List the license publisher type in the web site configuration file (Web.Config)

```xml
<system.serviceModel>
 <behaviors>
  <serviceBehaviors>
   <behavior name="SamplePublisher.SvcPublisherBehavior">
    <serviceMetadata httpGetEnabled="true" />
    <serviceDebug includeExceptionDetailInFaults="false" />
   </behavior>
  </serviceBehaviors>
 </behaviors>
 <services>
  <service behaviorConfiguration="SamplePublisher.SvcPublisherBehavior" name="SamplePublisher.SvcPublisher">
   <endpoint binding="wsHttpBinding" contract="DevZest.Licensing.ILicensePublisher" />
   <endpoint address="mex" binding="mexHttpBinding" contract="IMetadataExchange" />
  </service>
 </services>
</system.serviceModel>
```