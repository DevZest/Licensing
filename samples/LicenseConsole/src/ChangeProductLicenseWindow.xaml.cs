using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;
using DevZest.Licensing;
using System.Globalization;
using System.Collections.Specialized;

namespace LicenseConsole
{
    public partial class ChangeProductLicenseWindow : Window
    {
        public static readonly DependencyProperty LicenseTypeProperty = DependencyProperty.Register(
            "LicenseType", typeof(LicenseType), typeof(ChangeProductLicenseWindow),
            new FrameworkPropertyMetadata(LicenseType.Paid, new PropertyChangedCallback(OnLicenseTypeChanged)));
        public static readonly DependencyProperty LicenseMethodProperty = DependencyProperty.Register(
            "LicenseMethod", typeof(LicenseMethod), typeof(ChangeProductLicenseWindow),
            new FrameworkPropertyMetadata(LicenseMethod.WebService, new PropertyChangedCallback(OnLicenseMethodChanged)));
        private static readonly DependencyPropertyKey ValidationErrorPropertyKey = DependencyProperty.RegisterReadOnly(
            "ValidationError", typeof(ChangeProductLicenseValidationErrors), typeof(ChangeProductLicenseWindow),
            new FrameworkPropertyMetadata(ChangeProductLicenseValidationErrors.None));
        public static readonly DependencyProperty ValidationErrorProperty = ValidationErrorPropertyKey.DependencyProperty;

        private static void OnLicenseTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LicenseType newValue = (LicenseType)e.NewValue;
            if (newValue == LicenseType.Distributable)
                throw new ArgumentOutOfRangeException("e");

            ((ChangeProductLicenseWindow)d).Refresh();
        }

        private static void OnLicenseMethodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ChangeProductLicenseWindow)d).Refresh();
        }

        public ChangeProductLicenseWindow()
        {
            LicenseConsoleData data = LicenseConsoleData.Singleton;
            DataContext = data;
            InitializeComponent();
            UserName = data.LicenseEntry.Name;
            Company = data.LicenseEntry.Company;
            Email = data.LicenseEntry.Email;
            LicenseKey = data.LicenseEntry.LicenseKey;
            Refresh();
        }

        private LicenseConsoleData Data
        {
            get { return DataContext as LicenseConsoleData; }
        }

        public LicenseType LicenseType
        {
            get { return (LicenseType)GetValue(LicenseTypeProperty); }
            set { SetValue(LicenseTypeProperty, value); }
        }

        public LicenseMethod LicenseMethod
        {
            get { return (LicenseMethod)GetValue(LicenseMethodProperty); }
            set { SetValue(LicenseMethodProperty, value); }
        }

        public ChangeProductLicenseValidationErrors ValidationError
        {
            get { return (ChangeProductLicenseValidationErrors)GetValue(ValidationErrorProperty); }
            private set { SetValue(ValidationErrorPropertyKey, value); }
        }

        private LicenseKey LicenseKey
        {
            get
            {
                string s = _licenseKey1.Text + _licenseKey2.Text + _licenseKey3.Text + _licenseKey4.Text + _licenseKey5.Text;
                if (LicenseKey.CanConvertFrom(s))
                    return new LicenseKey(s);
                else
                    return LicenseKey.Empty;
            }
            set
            {
                string s = value.ToString();
                if (string.IsNullOrEmpty(s))
                    return;
                _licenseKey1.Text = s.Substring(0, 5);
                _licenseKey2.Text = s.Substring(6, 5);
                _licenseKey3.Text = s.Substring(12, 5);
                _licenseKey4.Text = s.Substring(18, 5);
                _licenseKey5.Text = s.Substring(24, 5);
            }
        }

        private string UserName
        {
            get { return GetTextBoxText(_textBoxName); }
            set { SetTextBoxText(_textBoxName, value); }
        }

        private string Company
        {
            get { return GetTextBoxText(_textBoxCompany); }
            set { SetTextBoxText(_textBoxCompany, value); }
        }

        private string Email
        {
            get { return GetTextBoxText(_textBoxEmail); }
            set { SetTextBoxText(_textBoxEmail, value); }
        }

        private static string GetTextBoxText(TextBox textBox)
        {
            return string.IsNullOrEmpty(textBox.Text) ? string.Empty : textBox.Text.Trim();
        }

        private static void SetTextBoxText(TextBox textBox, string value)
        {
            textBox.Text = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        private void RefreshOnTextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh();
        }

        private void RefreshOnAcceptLicenseAgreementChanged(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            ValidationError = RefreshValidationError();
        }

        private ChangeProductLicenseValidationErrors RefreshValidationError()
        {
            ChangeProductLicenseValidationErrors validationError = ChangeProductLicenseValidationErrors.None;

            if (!_checkBoxAcceptLicenseAgreement.IsChecked.HasValue || !_checkBoxAcceptLicenseAgreement.IsChecked.Value)
                validationError |= ChangeProductLicenseValidationErrors.AcceptLicenseAgreementRequired;

            if (string.IsNullOrEmpty(UserName))
                validationError |= ChangeProductLicenseValidationErrors.UserNameRequired;

            if (LicenseType == LicenseType.Paid && LicenseKey.IsEmpty)
                validationError |= ChangeProductLicenseValidationErrors.LicenseKeyRequired;

            if (LicenseMethod == LicenseMethod.WebService && !IsValidEmail(Email))
                validationError |= ChangeProductLicenseValidationErrors.EmailAddressRequired;

            return validationError;
        }

        private static bool IsValidEmail(string email)
        {
            Regex regex = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            return regex.IsMatch(email);
        }

        private void PurchaseUrl_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperLink = (Hyperlink)sender;
            Program.NavigateUri(hyperLink.NavigateUri);
        }

        private void LicenseAgreement_Click(object sender, RoutedEventArgs e)
        {
            LicenseAgreementWindow licenseAgreementWindow = new LicenseAgreementWindow();
            licenseAgreementWindow.Owner = this;
            licenseAgreementWindow.ShowDialog();
        }

        private bool ChangeProductLicense()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            bool succeed = Data.ChangeProductLicense(LicenseType, UserName, Company, Email, LicenseKey);
            Mouse.OverrideCursor = null;
            return succeed;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (LicenseMethod == LicenseMethod.WebService)
            {
                if (ChangeProductLicense())
                    Close();
                else
                    MessageBox.Show(Data.LastErrorMessage);
            }
            else
            {
                EmailLicenseRequestWindow.Show(this, LicenseType, LicenseKey, UserName, Company);
            }
        }
    }
}
