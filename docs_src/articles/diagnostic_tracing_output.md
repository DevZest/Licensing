---
uid: diagnostic_tracing_output
---

# Diagnostic Tracing Output

You can enable tracing to help diagnose problems for specified license provider or all license providers globally. .Net Licensing utilizes standard .Net application tracing. Once tracing is enabled, the tracing output will be displayed in the debug output window by default. You can also determine where you want the tracing output to appear by adding/removing the appropriate listeners. For more information, see .Net Framework documentation.

>[!Note]
>Enabling tracing may impact the performance. Disable tracing after you finished the troubleshooting.

## Enable Tracing for Specified License Provider

To enable tracing for specified license provider, set the license provider's `Debug` property to true:

```csharp
[assembly: FileLicenseProvider(Debug=true)]
```

```vb
<Assembly: FileLicenseProvider(Debug:=True)>
```

## Enable Tracing for All License Providers

To enable tracing for all license providers, add the following section to your application configuration file:

```xml
<system.diagnostics>
 <switches>
   <add name="DevZest.Licensing" value="true"/>
 </switches>
</system.diagnostics>
```