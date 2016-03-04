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
using System.Reflection;
using DevZest.Licensing;

namespace LicenseConsole
{
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        public string ProductVersion
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.ProductVersion;
            }
        }

        public Version FrameworkVersion
        {
            get { return Environment.Version; }
        }

        public IEnumerable<AssemblyName> References
        {
            get
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    yield return assembly.GetName();
            }
        }
    }
}
