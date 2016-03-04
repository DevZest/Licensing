using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using System.Configuration;
using System.Reflection;
using System.Globalization;
using DevZest.Licensing;

namespace LicenseConsole
{
    /// <summary>
    /// Interaction logic for ActivationWindow.xaml
    /// </summary>
    public partial class GetLicenseWindow : Window
    {
        public static readonly DependencyProperty LicenseMethodProperty = DependencyProperty.Register(
            "LicenseMethod", typeof(LicenseMethod), typeof(GetLicenseWindow), new FrameworkPropertyMetadata(LicenseMethod.WebService));
        private static readonly DependencyPropertyKey CanGetLicensePropertyKey = DependencyProperty.RegisterReadOnly(
            "CanGetLicense", typeof(bool), typeof(GetLicenseWindow), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty CanGetLicenseProperty = CanGetLicensePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CanSaveLicensePropertyKey = DependencyProperty.RegisterReadOnly(
            "CanSaveLicense", typeof(bool), typeof(GetLicenseWindow), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty CanSaveLicenseProperty = CanSaveLicensePropertyKey.DependencyProperty;

        public GetLicenseWindow()
        {
            InitializeComponent();
        }

        public LicenseMethod LicenseMethod
        {
            get { return (LicenseMethod)GetValue(LicenseMethodProperty); }
            set { SetValue(LicenseMethodProperty, value); }
        }

        public bool CanGetLicense
        {
            get { return (bool)GetValue(CanGetLicenseProperty); }
            private set { SetValue(CanGetLicensePropertyKey, value); }
        }

        public bool CanSaveLicense
        {
            get { return (bool)GetValue(CanSaveLicenseProperty); }
            private set { SetValue(CanSaveLicensePropertyKey, value); }
        }

        private string AssemblyPath
        {
            get { return File.Exists(_textBoxAssemblyPath.Text) ? _textBoxAssemblyPath.Text : string.Empty; }
            set { _textBoxAssemblyPath.Text = value; }
        }

        private string License
        {
            get { return _textBoxLicense.Text; }
            set
            {
                _textBoxLicense.Text = value;
                CanSaveLicense = !string.IsNullOrEmpty(value);
            }
        }

        private static LicenseConsoleData Data
        {
            get { return LicenseConsoleData.Singleton; }
        }

        private void AssemblyPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            CanGetLicense = !string.IsNullOrEmpty(AssemblyPath);
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();

            dialog.Filter = "Assemblies (*.dll, *.exe)|*.dll;*.exe";
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;
            dialog.FilterIndex = 1;
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                AssemblyPath = dialog.FileName;
        }

        private void Get_Click(object sender, RoutedEventArgs e)
        {
            string assemblyData;
            try
            {
                assemblyData = AssemblyLicense.GetAssemblyData(AssemblyPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (assemblyData == LicenseConsoleData.Singleton.AssemblyData)
            {
                MessageBox.Show(Strings.FormatDistributableLicenseForSelfComponent(assemblyData));
                return;
            }

            if (LicenseMethod == LicenseMethod.WebService)
            {
                License license = GetDistributableLicense(assemblyData);
                if (license == null)
                    MessageBox.Show(Data.LastErrorMessage);
                else
                    License = license.SignedString;
            }
            else
            {
                EmailLicenseRequestWindow.Show(this, AssemblyPath, assemblyData);
            }
        }

        private static License GetDistributableLicense(string assemblyData)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            License license = Data.GetDistributableLicense(assemblyData);
            Mouse.OverrideCursor = null;
            return license;
        }

        private static string LicenseFileName
        {
            get
            {
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(ConfigurationManager.AppSettings["Assembly"]);
                return Path.GetFileName(AssemblyLicenseProviderAttribute.GetAssemblyLicenseFileName(assemblyName));
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (SaveLicense(License))
                Close();
        }

        internal static bool SaveLicense(string license)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = Path.Combine(dialog.SelectedPath, LicenseFileName);
                if (File.Exists(filePath))
                {
                    if (MessageBox.Show(Strings.FormatOverwriteDistributableLicenseMessage(filePath),
                        Strings.GetCaption(CaptionId.OverwriteDistributableLicense),
                        MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return false;
                }
                try
                {
                    File.WriteAllText(filePath, license);
                    MessageBox.Show(Strings.FormatDistributableLicenseSaved(LicenseFileName, dialog.SelectedPath));
                    return true;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                    return false;
                }
            }

            return false;
        }

        internal static int GetAndSaveLicense(string assemblyPath, string outputPath, out string message)
        {
            AssemblyName callerAssembly;

            try
            {
                callerAssembly = AssemblyName.GetAssemblyName(assemblyPath);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return 1;
            }

            License license = Data.GetDistributableLicense(AssemblyLicense.GetAssemblyData(assemblyPath));
            if (license == null)
            {
                message = Data.LastErrorMessage;
                return 2;
            }

            try
            {
                string filePath = Path.Combine(outputPath, LicenseFileName);
                File.WriteAllText(filePath, license.SignedString);
                message = Strings.FormatDistributableLicenseSaved(LicenseFileName, outputPath);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return 3;
            }

            return 0;
        }
    }
}
