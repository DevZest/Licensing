using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.ServiceModel;
using System.IO;
using System.Text;
using System.Reflection;
using System.Web;
using System.Runtime.InteropServices;
using DevZest.Licensing;

namespace LicenseConsole
{

    internal static class Program
    {
        [STAThread()]
        static int Main(string[] args)
        {
            string errorMessage = LicenseConsoleData.Initialize();
            if (errorMessage != null)
            {
                MessageBox.Show(errorMessage);
                return 0;
            }

            LicenseError licenseError = DevZest.Licensing.LicenseManager.Check(".Net Licensing", typeof(LicenseClient));
            if (licenseError != null)
            {
                MessageBox.Show(licenseError.ExceptionMessage);
                return -1;
            }

            App.Main();
            return 0;
        }

        internal static String ConstructQueryString(NameValueCollection parameters)
        {
            List<String> items = new List<String>();

            foreach (string name in parameters)
            {
                string value = parameters[name];
                if (value == null)
                    value = string.Empty;
                items.Add(string.Concat(name, "=", Uri.EscapeDataString(value)));
            }

            return String.Join("&", items.ToArray());
        }

        internal static void NavigateUri(Uri uri)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Process.Start(uri.ToString());
            }
            catch (Win32Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}
