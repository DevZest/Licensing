using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using DevZest.Licensing;

namespace LicenseConsole
{
    public partial class EmailLicenseRequestWindow : Window
    {
        private static readonly DependencyPropertyKey MailtoPropertyKey = DependencyProperty.RegisterReadOnly("Mailto", typeof(string), typeof(EmailLicenseRequestWindow),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty MailtoProperty = MailtoPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey MessageBodyPropertyKey = DependencyProperty.RegisterReadOnly("MessageBody", typeof(string), typeof(EmailLicenseRequestWindow),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty MessageBodyProperty = MessageBodyPropertyKey.DependencyProperty;

        public EmailLicenseRequestWindow()
        {
            InitializeComponent();
        }

        public string Mailto
        {
            get { return (string)GetValue(MailtoProperty); }
            private set { SetValue(MailtoPropertyKey, value); }
        }

        public string MessageBody
        {
            get { return (string)GetValue(MessageBodyProperty); }
            private set { SetValue(MessageBodyPropertyKey, value); }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (!Owner.IsVisible)
                Owner.Close();
        }

        public static void Show(Window ownerWindow, LicenseType licenseType, LicenseKey licenseKey, string userName, string company)
        {
            NameValueCollection parameters = PrepareParameters(licenseType, licenseKey, userName, company, MachineLicense.LocalMachineData);
            string designTimeLicenseCategory = LicenseConsoleData.GetLicenseCategory(licenseType, true);
            if (!string.IsNullOrEmpty(designTimeLicenseCategory))
                parameters.Add("DesignTimeLicenseCategory", designTimeLicenseCategory);
            parameters.Add("RuntimeLicenseCategory", LicenseConsoleData.GetLicenseCategory(licenseType, false));
            Show(ownerWindow, parameters);
        }

        public static void Show(Window ownerWindow, string assemblyPath, string assemblyData)
        {
            LicenseEntry licenseEntry = LicenseConsoleData.Singleton.LicenseEntry;
            NameValueCollection parameters = PrepareParameters(LicenseType.Distributable, licenseEntry.LicenseKey, licenseEntry.Name, licenseEntry.Company, assemblyData);
            parameters.Add("AssemblyPath", assemblyPath);
            parameters.Add("LicenseCategory", LicenseConsoleData.GetLicenseCategory(LicenseType.Distributable));
            Show(ownerWindow, parameters);
        }

        private static NameValueCollection PrepareParameters(LicenseType licenseType, LicenseKey licenseKey, string userName, string company, string data)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("Culture", CultureInfo.CurrentCulture.LCID.ToString(CultureInfo.InvariantCulture));
            parameters.Add("Product", LicenseConsoleData.Singleton.Product);
            parameters.Add("Version", LicenseConsoleData.Singleton.Version.ToString());
            parameters.Add("LicenseType", licenseType.ToString());
            if (!licenseKey.IsEmpty)
                parameters.Add("LicenseKey", licenseKey.ToString());
            parameters.Add("UserName", userName);
            parameters.Add("Company", company);
            parameters.Add("Data", data);

            return parameters;
        }

        private static void Show(Window ownerWindow, NameValueCollection parameters)
        {
            EmailLicenseRequestWindow window = new EmailLicenseRequestWindow();
            window.Mailto = LicenseConsoleData.Singleton.LicenseEmail;
            window.MessageBody = Program.ConstructQueryString(parameters);
            window.Owner = ownerWindow;
            ownerWindow.Hide();
            window.ShowDialog();
        }

        private void CopyMailto_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, Mailto);
        }

        private void CopyMessageBody_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, MessageBody);
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            Owner.Visibility = Visibility.Visible;
            Close();
        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            string command = string.Format(CultureInfo.InvariantCulture, "mailto:{0}?subject={1}&body={2}", Uri.EscapeDataString(Mailto), Uri.EscapeDataString("License Reuqest"), Uri.EscapeDataString(MessageBody));
            try
            {
                Process.Start(command);
                Close();
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(Strings.FormatLaunchEmailClientErrorMessage(ex));
            }
        }
    }
}
