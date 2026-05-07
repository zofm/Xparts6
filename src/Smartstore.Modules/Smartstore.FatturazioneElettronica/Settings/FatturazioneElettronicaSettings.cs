using Smartstore.Core.Configuration;

namespace Smartstore.FatturazioneElettronica.Settings
{
    public class FatturazioneElettronicaSettings : ISettings
    {
        #region Aruba

        public string ArubaUsername { get; set; }

        public string ArubaPassword { get; set; }

        public string ArubaTaxCode { get; set; }

        public bool UseTestEnvironment { get; set; }

        public string WaitingFolderName => "Invoices\\_waitingForUpload";
        public string FailureFolderName => "Invoices\\_failures";
        public string DeliveredFolderName => "Invoices\\_delivered";
        public string DoneFolderName => "Invoices\\_done";

        public string BaseAuthUrl
        {
            get
            {
                if (UseTestEnvironment)
                    return "https://demoauth.fatturazioneelettronica.aruba.it";

                return "https://auth.fatturazioneelettronica.aruba.it";
            }
        }

        public string BaseUrl
        {
            get
            {
                if (UseTestEnvironment)
                    return "https://demows.fatturazioneelettronica.aruba.it";

                return "https://ws.fatturazioneelettronica.aruba.it";
            }
        }

        #endregion

        #region Common

        public string InvoiceNumberPattern => "X{0:0000}/{1}";

        public string AppDataFolder { get; set; }

        #endregion

        #region Tenant data

        public string EORI { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string AddressNumber { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string TaxCode { get; set; }
        public string VatCode { get; set; }

        #endregion
    }
}
