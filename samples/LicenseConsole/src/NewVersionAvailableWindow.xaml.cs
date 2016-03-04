using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Configuration;

namespace LicenseConsole
{
    /// <summary>
    /// Interaction logic for NewVersionAvailableWindow.xaml
    /// </summary>
    public partial class NewVersionAvailableWindow : Window
    {
        public NewVersionAvailableWindow()
        {
            DataContext = LicenseConsoleData.Singleton;
            InitializeComponent();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = (Hyperlink)sender;
            Program.NavigateUri(hyperlink.NavigateUri);
        }
    }
}
