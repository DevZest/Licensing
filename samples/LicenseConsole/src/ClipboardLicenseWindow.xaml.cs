using System;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Text;
using System.Web;
using System.Globalization;

namespace LicenseConsole
{
    /// <summary>
    /// Interaction logic for ClipboardLicenseWindow.xaml
    /// </summary>
    public partial class ClipboardLicenseWindow : Window
    {
        private const string BeginLicense = "========== BEGIN LICENSE ==========";
        private const string EndLicense = "========== END LICENSE ==========";

        private static readonly DependencyPropertyKey LogPropertyKey = DependencyProperty.RegisterReadOnly(
            "Log", typeof(string), typeof(ClipboardLicenseWindow), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty LogProperty = LogPropertyKey.DependencyProperty;

        public ClipboardLicenseWindow()
        {
            InitializeComponent();
        }

        public string Log
        {
            get { return (string)GetValue(LogProperty); }
            set { SetValue(LogPropertyKey, value); }
        }

        private static void Show(Window owner, string log)
        {
            ClipboardLicenseWindow window = new ClipboardLicenseWindow();
            window.Log = log;
            window.Owner = owner;
            window.ShowDialog();
        }

        public static bool CheckClipboard(Window owner, bool distributable)
        {
            if (string.IsNullOrEmpty(LicenseConsoleData.Singleton.LicenseEmail))
                return false;

            StringBuilder log;
            NameValueCollection parameters = CheckClipboard(distributable, out log);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Show(owner, log.ToString());
                return true;
            }

            if (parameters == null)
                return false;

            if (distributable)
                return ClipboardDistributableLicenseWindow.Show(owner, parameters);
            else
                return ClipboardProductLicenseWindow.Show(owner, parameters);
        }

        private static NameValueCollection CheckClipboard(bool distributable, out StringBuilder log)
        {
            log = new StringBuilder();

            string textOnClipboard = Clipboard.GetText();
            if (string.IsNullOrEmpty(textOnClipboard))
            {
                log.AppendLine("There is no text on clipboard.");
                return null;
            }

            int indexBeginLicense = textOnClipboard.IndexOf(BeginLicense);
            if (indexBeginLicense == -1)
            {
                log.AppendLine(string.Format(CultureInfo.InvariantCulture, "The Line \"{0}\" is not detected.", BeginLicense));
                return null;
            }
            int indexEndLicense = textOnClipboard.IndexOf(EndLicense);
            if (indexEndLicense == -1)
            {
                log.AppendLine(string.Format(CultureInfo.InvariantCulture, "The Line \"{0}\" is not detected.", EndLicense));
                return null;
            }

            indexBeginLicense += BeginLicense.Length;
            if (indexBeginLicense > indexEndLicense)
            {
                log.AppendLine("END LICENSE line appears before BEGIN LICENSE line.");
                return null;
            }

            string queryString = RemoveWhiteSpace(textOnClipboard.Substring(indexBeginLicense, indexEndLicense - indexBeginLicense));
            NameValueCollection parameters = HttpUtility.ParseQueryString(queryString);
            log.AppendLine("Detected License on Clipboard:");
            foreach (string key in parameters.AllKeys)
            {
                string value = parameters[key];
                log.Append(key);
                log.Append("=");
                if (value.IndexOf("\n") != -1)
                    log.AppendLine();
                log.AppendLine(value);
            }

            log.AppendLine();
            string productFullName = parameters["Product"];
            if (productFullName != LicenseConsoleData.Singleton.Product)
            {
                log.AppendLine("*** Invalid product.");
                return null;
            }

            string licenseType = parameters["LicenseType"];
            if (!IsValidLicenseType(licenseType, distributable))
            {
                log.AppendLine("*** Invalid license type.");
                return null;
            }

            return parameters;
        }

        private static bool IsValidLicenseType(string licenseType, bool distributable)
        {
            if (distributable)
                return licenseType == LicenseType.Distributable.ToString();
            else
                return licenseType == LicenseType.FreeFeature.ToString() ||
                    licenseType == LicenseType.Evaluation.ToString() ||
                    licenseType == LicenseType.Paid.ToString();
        }

        private static string RemoveWhiteSpace(string input)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (!char.IsWhiteSpace(c))
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }
    }
}
