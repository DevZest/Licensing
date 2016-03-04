using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Security.Principal;
using System.Reflection;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using DevZest.Licensing;
using System.ServiceModel;
using System.Threading;
using System.Windows.Threading;

namespace LicenseConsole
{
    public sealed class LicenseConsoleData : DependencyObject
    {
        private static readonly DependencyPropertyKey ProductPropertyKey = DependencyProperty.RegisterReadOnly(
            "Product", typeof(string), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ProductProperty = ProductPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey VersionPropertyKey = DependencyProperty.RegisterReadOnly(
            "Version", typeof(Version), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty VersionProperty = VersionPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey LicenseStatePropertyKey = DependencyProperty.RegisterReadOnly(
            "LicenseState", typeof(LicenseState), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(LicenseState.NotLicensed));
        public static readonly DependencyProperty LicenseStateProperty = LicenseStatePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey MainWindowTitlePropertyKey = DependencyProperty.RegisterReadOnly(
            "MainWindowTitle", typeof(string), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty MainWindowTitleProperty = MainWindowTitlePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey LicenseStateDescriptionPropertyKey = DependencyProperty.RegisterReadOnly(
            "LicenseStateDescription", typeof(string), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty LicenseStateDescriptionProperty = LicenseStateDescriptionPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey UserInfoPropertyKey = DependencyProperty.RegisterReadOnly(
            "UserInfo", typeof(string), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty UserInfoProperty = UserInfoPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey UserEmailPropertyKey = DependencyProperty.RegisterReadOnly(
            "UserEmail", typeof(string), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty UserEmailProperty = UserEmailPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsMachineLicenseEnabledPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsMachineLicenseEnabled", typeof(bool), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsMachineLicenseEnabledProperty = IsMachineLicenseEnabledPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CanGetFreeFeatureLicensePropertyKey = DependencyProperty.RegisterReadOnly(
            "CanGetFreeFeatureLicense", typeof(bool), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty CanGetFreeFeatureLicenseProperty = CanGetFreeFeatureLicensePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CanGetEvaluationLicensePropertyKey = DependencyProperty.RegisterReadOnly(
            "CanGetEvaluationLicense", typeof(bool), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty CanGetEvaluationLicenseProperty = CanGetEvaluationLicensePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CanGetPaidLicensePropertyKey = DependencyProperty.RegisterReadOnly(
            "CanGetPaidLicense", typeof(bool), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty CanGetPaidLicenseProperty = CanGetPaidLicensePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CanGetDistributableLicensePropertyKey = DependencyProperty.RegisterReadOnly(
            "CanGetDistributableLicense", typeof(bool), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty CanGetDistributableLicenseProperty = CanGetDistributableLicensePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey PurchaseUrlPropertyKey = DependencyProperty.RegisterReadOnly(
            "PurchaseUrl", typeof(Uri), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty PurchaseUrlProperty = PurchaseUrlPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey LicenseEmailPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "LicenseEmail", typeof(string), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty LicenseEmailProperty = LicenseEmailPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CanGetLicenseViaEmailPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "CanGetLicenseViaEmail", typeof(bool), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty CanGetLicenseViaEmailProperty = CanGetLicenseViaEmailPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CheckUpdateResultPropertyKey = DependencyProperty.RegisterReadOnly(
            "CheckUpdateResult", typeof(CheckUpdateResult), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(CheckUpdateResult.UpToDate));
        public static readonly DependencyProperty CheckUpdateResultProperty = CheckUpdateResultPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CheckUpdateErrorMessagePropertyKey = DependencyProperty.RegisterReadOnly(
            "CheckUpdateErrorMessage", typeof(string), typeof(LicenseConsoleData), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty CheckUpdateErrorMessageProperty = CheckUpdateResultPropertyKey.DependencyProperty;

        private string _assemblyData;
        public string AssemblyData
        {
            get { return _assemblyData; }
        }

        private string _publicKeyXml;
        private DateTime _releaseDate;
        private LicenseEntry _licenseEntry;
        private string _lastErrorMessage;
        private Dictionary<string, LicenseItemData> _licenseItems = new Dictionary<string, LicenseItemData>();

        private static LicenseConsoleData s_singleton;
        public static LicenseConsoleData Singleton
        {
            get { return s_singleton; }
        }

        public static string Initialize()
        {
            string assemblyFile = ConfigurationManager.AppSettings["Assembly"];
            AssemblyInfo assemblyInfo = new AssemblyInfo(assemblyFile);
            s_singleton = new LicenseConsoleData(assemblyInfo);
            return null;
        }

        private LicenseConsoleData(AssemblyInfo assemblyInfo)
        {
            PurchaseUrl = new Uri(ConfigurationManager.AppSettings["PurchaseUrl"]);
            LicenseEmail = ConfigurationManager.AppSettings["LicenseEmail"];
            CanGetLicenseViaEmail = !string.IsNullOrEmpty(LicenseEmail);

            Product = assemblyInfo.Product;
            Version = assemblyInfo.AssemblyFileVersion;
            _assemblyData = assemblyInfo.AssemblyData;
            string productTitle = Product;
            string licenseEntryFolder = ConfigurationManager.AppSettings["LicenseEntryFolder"];
            if (licenseEntryFolder.EndsWith(@"\", StringComparison.Ordinal))
                productTitle = productTitle + string.Format(CultureInfo.InvariantCulture, " {0}.{1}", Version.Major, Version.Minor);
            MainWindowTitle = Strings.FormatMainWindowTitle(productTitle);
            foreach (var pair in assemblyInfo.LicenseItems)
                _licenseItems.Add(pair.Key, new LicenseItemData(pair.Key, pair.Value));

            _publicKeyXml = assemblyInfo.PublicKeyXml;
            _releaseDate = assemblyInfo.ReleaseDate;

            CanGetFreeFeatureLicense = !string.IsNullOrEmpty(GetLicenseCategory(LicenseType.FreeFeature));
            CanGetEvaluationLicense = !string.IsNullOrEmpty(GetLicenseCategory(LicenseType.Evaluation));
            CanGetPaidLicense = !string.IsNullOrEmpty(GetLicenseCategory(LicenseType.Paid));

            Refresh();
        }

        public string Product
        {
            get { return (string)GetValue(ProductProperty); }
            private set { SetValue(ProductPropertyKey, value); }
        }

        public Version Version
        {
            get { return (Version)GetValue(VersionProperty); }
            private set { SetValue(VersionPropertyKey, value); }
        }

        public string UserInfo
        {
            get { return (string)GetValue(UserInfoProperty); }
            private set { SetValue(UserInfoPropertyKey, value); }
        }

        public string UserEmail
        {
            get { return (string)GetValue(UserEmailProperty); }
            private set { SetValue(UserEmailPropertyKey, value); }
        }

        public string MainWindowTitle
        {
            get { return (string)GetValue(MainWindowTitleProperty); }
            private set { SetValue(MainWindowTitlePropertyKey, value); }
        }

        public Uri PurchaseUrl
        {
            get { return (Uri)GetValue(PurchaseUrlProperty); }
            private set { SetValue(PurchaseUrlPropertyKey, value); }
        }

        public string LicenseEmail
        {
            get { return (string)GetValue(LicenseEmailProperty); }
            private set { SetValue(LicenseEmailPropertyKey, value); }
        }

        public bool CanGetLicenseViaEmail
        {
            get { return (bool)GetValue(CanGetLicenseViaEmailProperty); }
            private set { SetValue(CanGetLicenseViaEmailPropertyKey, value); }
        }

        public Dictionary<string, LicenseItemData>.ValueCollection LicenseItems
        {
            get { return _licenseItems.Values; }
        }

        public LicenseState LicenseState
        {
            get { return (LicenseState)GetValue(LicenseStateProperty); }
            private set { SetValue(LicenseStatePropertyKey, value); }
        }

        public string LicenseStateDescription
        {
            get { return (string)GetValue(LicenseStateDescriptionProperty); }
            private set { SetValue(LicenseStateDescriptionPropertyKey, value); }
        }

        public bool IsMachineLicenseEnabled
        {
            get { return (bool)GetValue(IsMachineLicenseEnabledProperty); }
            private set { SetValue(IsMachineLicenseEnabledPropertyKey, value); }
        }

        public bool CanGetFreeFeatureLicense
        {
            get { return (bool)GetValue(CanGetFreeFeatureLicenseProperty); }
            private set { SetValue(CanGetFreeFeatureLicensePropertyKey, value); }
        }

        public bool CanGetEvaluationLicense
        {
            get { return (bool)GetValue(CanGetEvaluationLicenseProperty); }
            private set { SetValue(CanGetEvaluationLicensePropertyKey, value); }
        }

        public bool CanGetPaidLicense
        {
            get { return (bool)GetValue(CanGetPaidLicenseProperty); }
            private set { SetValue(CanGetPaidLicensePropertyKey, value); }
        }

        public bool CanGetDistributableLicense
        {
            get { return (bool)GetValue(CanGetDistributableLicenseProperty); }
            private set { SetValue(CanGetDistributableLicensePropertyKey, value); }
        }

        public CheckUpdateResult CheckUpdateResult
        {
            get { return (CheckUpdateResult)GetValue(CheckUpdateResultProperty); }
            private set { SetValue(CheckUpdateResultPropertyKey, value); }
        }

        public string CheckUpdateErrorMessage
        {
            get { return (string)GetValue(CheckUpdateErrorMessageProperty); }
            private set { SetValue(CheckUpdateErrorMessagePropertyKey, value); }
        }

        public string LastErrorMessage
        {
            get { return _lastErrorMessage; }
            private set { _lastErrorMessage = value; }
        }

        internal LicenseEntry LicenseEntry
        {
            get { return _licenseEntry; }
        }

        internal void Refresh()
        {
            _licenseEntry = LicenseEntry.Load();
            UserInfo = GetUserInfo(_licenseEntry.Name, _licenseEntry.Company);
            UserEmail = _licenseEntry.Email;
            IsMachineLicenseEnabled = _licenseEntry.RuntimeLicense != null;
            RefreshLicenseState();
            RefreshLicenseItems();
        }

        private static string GetUserInfo(string name, string company)
        {
            if (string.IsNullOrEmpty(name))
                return company;
            else
                return string.IsNullOrEmpty(company) ? name : string.Format(CultureInfo.InvariantCulture, "{0}, {1}", name, company);
        }

        public License License
        {
            get { return _licenseEntry.RuntimeLicense != null ? _licenseEntry.RuntimeLicense : _licenseEntry.DisabledRuntimeLicense; }
        }

        private void RefreshLicenseState()
        {
            if (License == null)
                LicenseState = LicenseState.NotLicensed;
            else if (License.Category == GetLicenseCategory(LicenseType.Evaluation))
            {
                if (License.IsExpired)
                    LicenseState = LicenseState.EvaluationExpired;
                else
                    LicenseState = LicenseState.Evaluation;
            }
            else
                LicenseState = _licenseEntry.LicenseKey.IsEmpty ? LicenseState.FreeFeature : LicenseState.Paid;

            LicenseStateDescription = Strings.FormatLicenseStateDescription(LicenseState, License);        }

        private void RefreshLicenseItems()
        {
            bool canGetDistributableLicense = false;
            foreach (LicenseItemData data in LicenseItems)
            {
                if (License == null)
                {
                    data.State = LicenseItemState.Blocked;
                    continue;
                }

                LicenseItem licenseItem = License.Items[data.Name];
                if (licenseItem == null)
                    data.State = LicenseItemState.Blocked;
                else if (LicenseState == LicenseState.Evaluation || LicenseState == LicenseState.EvaluationExpired)
                {
                    if (licenseItem.OverrideExpirationDate)
                        data.State = LicenseItemState.Granted;
                    else if (LicenseState == LicenseState.Evaluation)
                        data.State = LicenseItemState.Evaluated;
                    else
                        data.State = LicenseItemState.Blocked;
                }
                else
                    data.State = LicenseItemState.Granted;

                if (data.State == LicenseItemState.Granted)
                    canGetDistributableLicense = true;
            }
            CanGetDistributableLicense = canGetDistributableLicense;
        }

        internal static string GetLicenseCategory(LicenseType licenseType)
        {
            return GetLicenseCategory(licenseType, false);
        }

        internal static string GetLicenseCategory(LicenseType licenseType, bool designMode)
        {
            string key = GetLicenseCategoryKey(licenseType, designMode);
            return ConfigurationManager.AppSettings[key];
        }

        private static string GetLicenseCategoryKey(LicenseType licenseType, bool designMode)
        {
            switch (licenseType)
            {
                case LicenseType.FreeFeature:
                    return designMode ? "DesignTimeFreeFeatureLicenseCategory" : "RuntimeFreeFeatureLicenseCategory";

                case LicenseType.Evaluation:
                    return designMode ? "DesignTimeEvaluationLicenseCategory" : "RuntimeEvaluationLicenseCategory";

                case LicenseType.Paid:
                    return designMode ? "DesignTimePaidLicenseCategory" : "RuntimePaidLicenseCategory";

                default:
                    Debug.Assert(licenseType == LicenseType.Distributable && designMode == false);
                    return "DistributableLicenseCategory";
            }
        }

        private bool CheckLicense(LicensePublisherResponse response)
        {
            License license = response.License;
            if (license == null)
            {
                LastErrorMessage = response.ErrorMessage;
                return false;
            }
            else if (!CheckUpgradeExpirationDate(license))
            {
                LastErrorMessage = Strings.GetMessage(MessageId.InvalidUpgradeExpirationDate);
                return false;
            }
            else
                return true;
        }

        private bool CheckUpgradeExpirationDate(License license)
        {
            return _releaseDate <= license.UpgradeExpirationDate;
        }

        public bool ChangeProductLicense(LicenseType licenseType, string userName, string company, string email, LicenseKey licenseKey)
        {
            if (licenseType == LicenseType.FreeFeature)
                return SetProductLicense(LicenseKey.Empty, userName, company, email, GetLicenseCategory(LicenseType.FreeFeature, true), GetLicenseCategory(LicenseType.FreeFeature));
            else if (licenseType == LicenseType.Evaluation)
                return SetProductLicense(LicenseKey.Empty, userName, company, email, GetLicenseCategory(LicenseType.Evaluation, true), GetLicenseCategory(LicenseType.Evaluation));
            else
            {
                Debug.Assert(licenseType == LicenseType.Paid);
                return SetProductLicense(licenseKey, userName, company, email, GetLicenseCategory(LicenseType.Paid, true), GetLicenseCategory(LicenseType.Paid));
            }
        }

        private bool SetProductLicense(LicenseKey licenseKey, string userName, string company, string email, string designTimeLicenseCategory, string runtimeLicenseCategory)
        {
            Debug.Assert(!string.IsNullOrEmpty(runtimeLicenseCategory));

            License designTimeLicense = null;
            License runtimeLicense;

            LicenseClient client = new LicenseClient(_publicKeyXml);
            try
            {
                LicensePublisherResponse response;
                if (!string.IsNullOrEmpty(designTimeLicenseCategory))
                {
                    response = client.GetLicense(CultureInfo.CurrentCulture, Product, Version, licenseKey, designTimeLicenseCategory, userName, company, email, MachineLicense.LocalMachineData);
                    if (!CheckLicense(response))
                    {
                        client.Close();
                        return false;
                    }
                    designTimeLicense = response.License;
                }
                response = client.GetLicense(CultureInfo.CurrentCulture, Product, Version, licenseKey, runtimeLicenseCategory, userName, company, email, MachineLicense.LocalMachineData);
                client.Close();
                if (!CheckLicense(response))
                    return false;
                runtimeLicense = response.License;
            }
            catch (CommunicationException exception)
            {
                LastErrorMessage = exception.Message;
                client.Abort();
                return false;
            }

            string designTimeLicenseString = designTimeLicense == null ? string.Empty : designTimeLicense.SignedString;
            string runtimeLicenseString = runtimeLicense == null ? string.Empty : runtimeLicense.SignedString;
            SaveProductLicense(licenseKey, userName, company, email, designTimeLicenseString, runtimeLicenseString);
            return true;
        }

        internal void SaveProductLicense(LicenseKey licenseKey, string userName, string company, string email, string designTimeLicense, string runtimeLicense)
        {
            LicenseEntry.Save(licenseKey, userName, company, email, designTimeLicense, runtimeLicense);
            Refresh();
        }

        public void ToggleRuntimeLicense()
        {
            LicenseEntry.ToggleRuntimeLicense();
            Refresh();
        }

        public License GetDistributableLicense(string assemblyData)
        {
            LicenseClient client = new LicenseClient(_publicKeyXml);
            try
            {
                {
                    LicensePublisherResponse response = client.GetLicense(CultureInfo.CurrentCulture, Product, Version, _licenseEntry.LicenseKey, GetLicenseCategory(LicenseType.Distributable), LicenseEntry.Name, LicenseEntry.Company, LicenseEntry.Email, assemblyData);
                    client.Close();
                    if (!CheckLicense(response))
                        return null;
                    else
                        return response.License;
                }
            }
            catch (CommunicationException ex)
            {
                LastErrorMessage = ex.Message;
                client.Abort();
                return null;
            }
        }

        private Collection<ProductRelease> _releases;
        public Collection<ProductRelease> Releases
        {
            get { return _releases; }
        }

        internal void BeginCheckUpdate()
        {
            string releasesUrl = ConfigurationManager.AppSettings["ProductReleasesUrl"];
            if (string.IsNullOrEmpty(releasesUrl))
                return;
            new Thread((ThreadStart)delegate { CheckUpdate(releasesUrl); }).Start();
        }

        private void CheckUpdate(string releasesUrl)
        {
            try
            {
                _releases = GetReleases(SafeGetVersion(), releasesUrl);
                if (_releases.Count > 0)
                    SafeSetNewVersionAvailable();
            }
            catch (Exception ex)
            {
                SafeSetCheckUpdateFailedResult(ex.Message);
            }
        }

        private delegate Version GetVersionDelegate();
        private delegate ProductRelease CreateProductReleaseDelegate();

        private Version SafeGetVersion()
        {
            if (!Dispatcher.CheckAccess())
                return (Version)Dispatcher.Invoke(DispatcherPriority.Send, (GetVersionDelegate)delegate { return SafeGetVersion(); });
            else
                return Version;
        }

        private void SafeSetNewVersionAvailable()
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(DispatcherPriority.Send, (ThreadStart)delegate { SafeSetNewVersionAvailable(); });
            else
                CheckUpdateResult = CheckUpdateResult.NewVersionAvailable;
        }

        private void SafeSetCheckUpdateFailedResult(string errorMessage)
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(DispatcherPriority.Send, (ThreadStart)delegate { SafeSetCheckUpdateFailedResult(errorMessage); });
            else
            {
                CheckUpdateResult = CheckUpdateResult.Failed;
                CheckUpdateErrorMessage = errorMessage;
            }
        }

        private ProductRelease SafeCreateProductReleaseObject(Version version, Uri downloadUrl, string releaseNotes)
        {
            if (!Dispatcher.CheckAccess())
                return (ProductRelease)Dispatcher.Invoke(DispatcherPriority.Send, (CreateProductReleaseDelegate)delegate { return SafeCreateProductReleaseObject(version, downloadUrl, releaseNotes); });
            else
                return new ProductRelease(version, downloadUrl, releaseNotes);
        }

        private Collection<ProductRelease> GetReleases(Version currentVersion, string productReleaseUrl)
        {
            Collection<ProductRelease> releases = new Collection<ProductRelease>();

            using (XmlReader xmlReader = XmlReader.Create(productReleaseUrl))
            {
                xmlReader.MoveToContent();

                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "Release")
                    {
                        xmlReader.MoveToAttribute("Version");
                        Version version = new Version(xmlReader.Value);
                        if (version <= currentVersion)
                            return releases;
                        xmlReader.MoveToAttribute("DownloadUrl");
                        Uri downloadUrl = new Uri(xmlReader.Value);
                        xmlReader.MoveToAttribute("ReleaseNotes");
                        string releaseNotes = Uri.UnescapeDataString(xmlReader.Value);
                        releases.Add(SafeCreateProductReleaseObject(version, downloadUrl, releaseNotes));
                    }
                }
            }

            return releases;
        }

        internal static bool IsAssemblySigned(AssemblyName assembly)
        {
            byte[] publicKeyToken = assembly.GetPublicKeyToken();
            return publicKeyToken != null && publicKeyToken.Length != 0;
        }
    }
}
