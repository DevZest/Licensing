using System;
using System.Diagnostics;
using System.Collections.Generic;
using DevZest.Licensing;
using System.IO;
using System.Web.Services;
using System.Reflection;
using System.Globalization;

namespace SamplePublisher
{
    public class Publisher : LicensePublisher
    {
        // The AssemblyProduct attribute of SampleApp
        private const string Product = "SampleApp";

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

        private static LicenseKey[] s_licenseKeys = {
        new LicenseKey("6VL8P-QHCRS-PF2GJ-8XLYG-VKHH4"),
        new LicenseKey("ZSWPT-Q3QV8-V9MM9-4WVFA-9Z9CQ"),
        new LicenseKey("XYDSY-LSKC5-9XVSJ-3QYLT-MALBQ") };

        protected override string GetPrivateKeyXml(string product)
        {
            // Load the private key file from current assembly's embedded resource.
            // Note the resource name is namespace + file name, and it's CASE SENSITIVE.
            // If wrong resource name provided, PrivateKeyXmlFromSnkFile throws ArgumentNullException!
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SamplePublisher.Key.snk"))
            {
                return PrivateKeyXmlFromSnkFile(stream);
            }

        }

        protected override LicensePublisherResponse GetLicense(CultureInfo cultureInfo, string product, Version version, LicenseKey licenseKey, string category, string userName, string company, string emailAddress, string data)
        {
            // Check the product
            if (product != Product)
                return new LicensePublisherResponse("Invalid product!");

            // Check the license key and category
            LicenseCategory? licenseCategory = null;
            for (int i = 0; i < s_licenseKeys.Length; i++)
            {
                if (s_licenseKeys[i] == licenseKey)
                {
                    licenseCategory = (LicenseCategory)i;
                    break;
                }
            }
            if (!licenseCategory.HasValue)
                return new LicensePublisherResponse("Invalid license key!");
            if (licenseCategory.ToString() != category)
                return new LicensePublisherResponse("Invalid category!");

            License license = new MachineLicense();
            license.Product = Product;
            license.Category = category;
            license.UserName = userName;
            license.Company = company;
            license.Data = data;
            license.Items.Add(new LicenseItem(LicenseItems.Feature1, true));  // Feature1's OverrideExpirationDate is true
            license.Items.Add(new LicenseItem(LicenseItems.Feature2));

            if (licenseCategory == LicenseCategory.Evaluation)
                license.SetExpirationDate(DateTime.UtcNow.AddMonths(3));
            else if (licenseCategory == LicenseCategory.ExpiredEvaluation)
                license.SetExpirationDate(DateTime.UtcNow); // Set ExpirationDate to now always expires the license

            return new LicensePublisherResponse(license);
        }
    }
}
