<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="TestServer._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <p>The License Publisher web service is up and running.</p>
        <p>Any valid license key is acceptable, for example: <% = DevZest.Licensing.LicenseKey.NewLicenseKey().ToString() %></p>
        <p><a href="LicensePublishService.asmx">Go to web service page</a></p>
    </div>
    </form>
</body>
</html>
