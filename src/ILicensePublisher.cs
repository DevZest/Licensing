using System.Globalization;
using System.ServiceModel;

namespace DevZest.Licensing
{
    /// <summary>Service contract between <see cref="LicensePublisher" /> and <see cref="LicenseClient" />.</summary>
    [ServiceContract(Namespace = "http://services.devzest.com/Licensing", Name = "LicensePublisher")]
    public interface ILicensePublisher
    {
        /// <summary>Publishes the requested license.</summary>
        /// <param name="culture">The license client culture LCID.</param>
        /// <param name="product">The product name.</param>
        /// <param name="version">The product version.</param>
        /// <param name="encryptedLicenseKey">The encrypted license key.</param>
        /// <param name="category">The license category.</param>
        /// <param name="name">The user name.</param>
        /// <param name="company">The user company.</param>
        /// <param name="email">The user email address.</param>
        /// <param name="data">The data to set for the license.</param>
        /// <returns>The response sent to license client.</returns>
        [OperationContract]
        string Publish(int culture, string product, string version, string encryptedLicenseKey, string category, string name, string company, string email, string data);
    }
}
