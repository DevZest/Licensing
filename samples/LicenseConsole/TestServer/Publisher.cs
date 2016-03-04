using System;
using System.Diagnostics;
using System.Collections.Generic;
using DevZest.Licensing;
using System.IO;
using System.Web.Services;
using System.Reflection;
using System.Globalization;

namespace TestServer
{
    public class Publisher : LicensePublisher
    {
        // The license item names
        private static class LicenseItems
        {
            public const string Feature1 = "Feature1";
            public const string Feature2 = "Feature2";
        }

        private enum LicenseCategory
        {
            ExpiredEvaluation = 0,
            Evaluation,
            Registered
        }

        protected override string GetPrivateKeyXml(string product)
        {
            // Load the private key file from current assembly's embedded resource.
            // Note the resource name is namespace + file name, and it's CASE SENSITIVE.
            // If wrong resource name provided, PrivateKeyXmlFromSnkFile throws ArgumentNullException!        
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TestServer.Key.snk"))
            {
                return PrivateKeyXmlFromSnkFile(stream);
            }

        }

        protected override LicensePublisherResponse GetLicense(CultureInfo cultureInfo, string product, Version version, LicenseKey licenseKey, string category, string userName, string company, string emailAddress, string data)
        {
            if (category == "Distributable")
            {
                AssemblyLicense license = new AssemblyLicense();
                license.Product = product;
                license.Category = category;
                license.UserName = userName;
                license.Company = company;
                license.Data = data;
                license.Items.Add(new LicenseItem(LicenseItems.Feature1));
                license.Items.Add(new LicenseItem(LicenseItems.Feature2));
                license.SetUpgradeExpirationDate(new DateTime(2999, 12, 31));

                return new LicensePublisherResponse(license);
            }
            else
            {
                MachineLicense license = new MachineLicense();
                license.Product = product;
                license.Category = category;
                license.UserName = userName;
                license.Company = company;
                license.Data = data;
                license.Items.Add(new LicenseItem(LicenseItems.Feature1));
                license.Items.Add(new LicenseItem(LicenseItems.Feature2));
                if (category == "Evaluation")
                    license.SetExpirationDate(DateTime.UtcNow.AddMonths(3));
                license.SetUpgradeExpirationDate(new DateTime(2999, 12, 31));

                return new LicensePublisherResponse(license);
            }
        }
    }
}