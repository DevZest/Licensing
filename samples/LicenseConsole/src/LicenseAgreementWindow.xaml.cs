using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Configuration;

namespace LicenseConsole
{
    public partial class LicenseAgreementWindow : Window
    {
        public LicenseAgreementWindow()
        {
            InitializeComponent();
        }

        private static string LicenseAgreementFile
        {
            get { return ConfigurationManager.AppSettings["licenseAgreementFile"]; }
        }

        private void RichTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(!string.IsNullOrEmpty(LicenseAgreementFile));

            TextRange textRange = new TextRange(_richTextBox.Document.ContentStart, _richTextBox.Document.ContentEnd);
            
            using (FileStream stream = new FileStream(LicenseAgreementFile, FileMode.Open, FileAccess.Read))
            {
                textRange.Load(stream, DataFormats.Rtf);
            }
        }
    }
}
