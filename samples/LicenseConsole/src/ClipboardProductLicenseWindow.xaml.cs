using System;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Windows;
using DevZest.Licensing;

namespace LicenseConsole
{
    /// <summary>
    /// Interaction logic for ClipboardProductLicenseWindow.xaml
    /// </summary>
    public partial class ClipboardProductLicenseWindow : Window
    {
        private static readonly DependencyPropertyKey LicenseTypePropertyKey = DependencyProperty.RegisterReadOnly(
            "LicenseType", typeof(LicenseType), typeof(ClipboardProductLicenseWindow), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty LicenseTypeProperty = LicenseTypePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey UserNamePropertyKey = DependencyProperty.RegisterReadOnly(
            "UserName", typeof(string), typeof(ClipboardProductLicenseWindow), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty UserNameProperty = UserNamePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey CompanyPropertyKey = DependencyProperty.RegisterReadOnly(
            "Company", typeof(string), typeof(ClipboardProductLicenseWindow), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty CompanyProperty = CompanyPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey EmailPropertyKey = DependencyProperty.RegisterReadOnly(
            "Email", typeof(string), typeof(ClipboardProductLicenseWindow), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty EmailProperty = EmailPropertyKey.DependencyProperty;

        private LicenseKey _licenseKey = LicenseKey.Empty;
        private string _designTimeLicense, _runtimeLicense;

        public ClipboardProductLicenseWindow()
        {
            InitializeComponent();
        }

        public LicenseType LicenseType
        {
            get { return (LicenseType)GetValue(LicenseTypeProperty); }
            private set { SetValue(LicenseTypePropertyKey, value); }
        }

        public string UserName
        {
            get { return (string)GetValue(UserNameProperty); }
            private set { SetValue(UserNamePropertyKey, value); }
        }

        public string Company
        {
            get { return (string)GetValue(CompanyProperty); }
            private set { SetValue(CompanyPropertyKey, value); }
        }

        public string Email
        {
            get { return (string)GetValue(EmailProperty); }
            private set { SetValue(EmailPropertyKey, value); }
        }

        public static bool Show(Window owner, NameValueCollection parameters)
        {
            ClipboardProductLicenseWindow window = new ClipboardProductLicenseWindow();
            window.LicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), parameters["LicenseType"]);
            window.UserName = parameters["UserName"];
            window.Company = parameters["Company"];
            window.Email = parameters["Email"];
            if (!string.IsNullOrEmpty(parameters["LicenseKey"]))
                window._licenseKey = new LicenseKey(parameters["LicenseKey"]);
            window._designTimeLicense = parameters["DesignTimeLicense"];
            window._runtimeLicense = parameters["RuntimeLicense"];
            window.Owner = owner;

            bool? result = window.ShowDialog();
            if (result.HasValue && result.Value)
                return true;
            else
                return false;
        }

        private static LicenseConsoleData Data
        {
            get { return LicenseConsoleData.Singleton; }
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            Data.SaveProductLicense(_licenseKey, UserName, Company, Email, _designTimeLicense, _runtimeLicense);
            Clipboard.Clear();
            DialogResult = true;
            Close();
        }
    }
}
