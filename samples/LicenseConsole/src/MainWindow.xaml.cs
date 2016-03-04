using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LicenseConsole
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            LicenseConsoleData data = LicenseConsoleData.Singleton;
            DataContext = data;
            InitializeComponent();
            data.BeginCheckUpdate();
        }

        private LicenseConsoleData Data
        {
            get { return DataContext as LicenseConsoleData; }
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            if (ClipboardLicenseWindow.CheckClipboard(this, false))
                return;

            ChangeProductLicenseWindow changeProductLicenseWindow = new ChangeProductLicenseWindow();
            changeProductLicenseWindow.Owner = this;
            changeProductLicenseWindow.ShowDialog();
        }

        private void GetLicense_Click(object sender, RoutedEventArgs e)
        {
            if (ClipboardLicenseWindow.CheckClipboard(this, true))
                return;

            GetLicenseWindow getLicenseWindow = new GetLicenseWindow();
            getLicenseWindow.Owner = this;
            getLicenseWindow.ShowDialog();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Data.ToggleRuntimeLicense();
        }

        private void DotNetLicensing_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperLink = (Hyperlink)sender;
            Program.NavigateUri(hyperLink.NavigateUri);
        }

        private void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            CheckUpdateWindow window = new CheckUpdateWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void NewVersionAvailable_Click(object sender, RoutedEventArgs e)
        {
            NewVersionAvailableWindow window = new NewVersionAvailableWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About window = new About();
            window.Owner = this;
            window.ShowDialog();
        }
    }
}
