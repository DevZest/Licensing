Imports System
Imports System.IO
Imports System.Reflection
Imports System.ServiceModel
Imports DevZest.Licensing

<Assembly: FileLicenseProvider()> 

Module Module1

    ' The AssemblyProduct attribute of SampleApp
    Private Const Product As String = "SampleApp"

    ' The license item names
    Private NotInheritable Class LicenseItems
        Public Const Feature1 As String = "Feature1"
        Public Const Feature2 As String = "Feature2"
    End Class

    Private Enum LicenseCategory
        ExpiredEvaluation = 0
        Evaluation
        Registered
    End Enum

    Private s_licenseKeys As LicenseKey() = { _
        New LicenseKey("6VL8P-QHCRS-PF2GJ-8XLYG-VKHH4"), _
        New LicenseKey("ZSWPT-Q3QV8-V9MM9-4WVFA-9Z9CQ"), _
        New LicenseKey("XYDSY-LSKC5-9XVSJ-3QYLT-MALBQ")}

    Sub Main()
        Output()
        Dim iInput As Integer = Input()
        While iInput <> 0
            GetLicense(CType(iInput - 1, LicenseCategory))
            Output()
            iInput = Input()
        End While
    End Sub

    Private Function Input() As Integer
        Dim categories As String() = [Enum].GetNames(GetType(LicenseCategory))

        For i As Integer = 0 To categories.Length - 1
            Console.WriteLine(String.Format("{0}: {1} ({2})", i + 1, categories(i), s_licenseKeys(i)))
        Next
        Console.WriteLine("0: Exit")
        While (True)
            Console.Write("Please Enter: ")
            Dim strInput As String = Console.ReadLine()
            Try
                Dim returnValue As Integer = Convert.ToInt32(strInput)
                If returnValue >= 0 And returnValue <= categories.Length Then
                    Return returnValue
                End If
            Catch exception As FormatException
            End Try
        End While
    End Function

    Private Sub Output()
        Console.WriteLine()
        Dim lic As License = LicenseManager.GetLicense()
        If lic Is Nothing Then
            Console.WriteLine("This application is not licensed")
        Else
            Console.WriteLine("License of this application:")
            Console.WriteLine(lic.SignedString)
            OutputLicenseItemValidation(LicenseItems.Feature1)
            OutputLicenseItemValidation(LicenseItems.Feature2)
        End If

        Console.WriteLine()
    End Sub

    Private Sub OutputLicenseItemValidation(ByVal licenseItemName As String)
        Dim licError As LicenseError = LicenseManager.Check(licenseItemName)
        Console.Write(licenseItemName + ": ")
        If licError Is Nothing Then
            Console.WriteLine("A valid license granted")
        Else
            Console.WriteLine(String.Format("Cannot grant a valid license, reason=""{0}"", message=""{1}""", licError.Reason, licError.Message))
        End If
    End Sub

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

End Module
