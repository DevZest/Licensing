using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace LicenseConsole
{
    /// <summary>
    /// Interaction logic for CheckUpdateWindow.xaml
    /// </summary>
    public partial class CheckUpdateWindow : Window
    {
        public CheckUpdateWindow()
        {
            DataContext = LicenseConsoleData.Singleton;
            InitializeComponent();
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            Program.NavigateUri(new Uri(ConfigurationManager.AppSettings["ProductReleasesWebPageUrl"]));
            Close();
        }
    }
}
