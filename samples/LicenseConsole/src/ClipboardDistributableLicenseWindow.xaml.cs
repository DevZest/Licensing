using System;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Windows;

namespace LicenseConsole
{
    /// <summary>
    /// Interaction logic for ClipboardDistributableLicenseWindow.xaml
    /// </summary>
    public partial class ClipboardDistributableLicenseWindow : Window
    {
        private static readonly DependencyPropertyKey AssemblyPathPropertyKey = DependencyProperty.RegisterReadOnly(
            "AssemblyPath", typeof(string), typeof(ClipboardDistributableLicenseWindow), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty AssemblyPathProperty = AssemblyPathPropertyKey.DependencyProperty;

        private string _license;

        public ClipboardDistributableLicenseWindow()
        {
            InitializeComponent();
        }

        public string AssemblyPath
        {
            get { return (string)GetValue(AssemblyPathProperty); }
            private set { SetValue(AssemblyPathPropertyKey, value); }
        }

        public static bool Show(Window owner, NameValueCollection parameters)
        {
            ClipboardDistributableLicenseWindow window = new ClipboardDistributableLicenseWindow();
            window.AssemblyPath = parameters["AssemblyPath"];
            window._license = parameters["License"];
            window.Owner = owner;
            bool? result = window.ShowDialog();
            if (result.HasValue && result.Value)
                return true;
            else
                return false;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            if (GetLicenseWindow.SaveLicense(_license))
            {
                Clipboard.Clear();
                DialogResult = true;
                Close();
            }
        }
    }
}
