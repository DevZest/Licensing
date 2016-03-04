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

    'The AssemblyProduct attribute of SampleApp
    Private Const Product As String = "SampleApp"

    'The license item names
    Private NotInheritable Class LicenseItems
        Public Const Feature1 As String = "Feature1"
        Public Const Feature2 As String = "Feature2"
    End Class

    Private Enum LicenseCategory
        ExpiredEvaluation = 0
        Evaluation
        Registered
    End Enum


    Private Shared s_licenseKeys As LicenseKey() = { _
        New LicenseKey("6VL8P-QHCRS-PF2GJ-8XLYG-VKHH4"), _
        New LicenseKey("ZSWPT-Q3QV8-V9MM9-4WVFA-9Z9CQ"), _
        New LicenseKey("XYDSY-LSKC5-9XVSJ-3QYLT-MALBQ")}

    Protected Overrides Function GetPrivateKeyXml(ByVal product As String) As String
        ' Load the private key file from current assembly's embedded resource.
        ' Note the resource name is namespace + file name, and it's CASE SENSITIVE.
        ' If wrong resource name provided, PrivateKeyXmlFromSnkFile throws ArgumentNullException!
        Using stream As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SamplePublisher.Key.snk")
            Return PrivateKeyXmlFromSnkFile(stream)
        End Using
    End Function

    Protected Overrides Function GetLicense(ByVal cultureInfo As CultureInfo, ByVal product As String, ByVal version As Version, ByVal licenseKey As LicenseKey, ByVal category As String, ByVal userName As String, ByVal company As String, ByVal emailAddress As String, ByVal data As String) As LicensePublisherResponse
        'Check the product
        If product <> Publisher.Product Then
            Return New LicensePublisherResponse("Invalid product!")
        End If

        'Check the license key and category
        Dim licenseCategory As Nullable(Of LicenseCategory) = Nothing
        For i As Integer = 0 To s_licenseKeys.Length - 1
            If s_licenseKeys(i) = licenseKey Then
                licenseCategory = CType(i, LicenseCategory)
                Exit For
            End If
        Next

        If Not licenseCategory.HasValue Then
            Return New LicensePublisherResponse("Invalid license key!")
        End If
        If licenseCategory.ToString() <> category Then
            Return New LicensePublisherResponse("Invalid category!")
        End If

        Dim license As License = New MachineLicense()
        license.Product = product
        license.Category = category
        license.UserName = userName
        license.Company = company
        license.Data = data
        license.Items.Add(New LicenseItem(LicenseItems.Feature1, True))  'Feature1's OverrideExpirationDate is true
        license.Items.Add(New LicenseItem(LicenseItems.Feature2))

        If licenseCategory = Publisher.LicenseCategory.Evaluation Then
            license.SetExpirationDate(DateTime.UtcNow.AddMonths(3))
        ElseIf licenseCategory = Publisher.LicenseCategory.ExpiredEvaluation Then
            license.SetExpirationDate(DateTime.UtcNow) 'Set ExpirationDate to now always expires the license
        End If

        Return New LicensePublisherResponse(license)
    End Function

End Class
