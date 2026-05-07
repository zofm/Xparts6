using System.ComponentModel.DataAnnotations;

namespace Smartstore.FatturazioneElettronica.Models
{
    public class ConfigurationModel : ModelBase
    {
        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.AppDataFolder")]
        [Required]
        public string AppDataFolder { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.ArubaUsername")]
        public string ArubaUsername { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.ArubaPassword")]
        public string ArubaPassword { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.ArubaTaxCode")]
        [StringLength(16)]
        public string ArubaTaxCode { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.UseTestEnvironment")]
        public bool UseTestEnvironment { get; set; }

        [Required]
        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.EORI")]
        public string EORI { get; set; }

        [Required]
        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.CompanyName")]
        public string CompanyName { get; set; }

        [Required]
        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.Address")]
        public string Address { get; set; }

        [Required]
        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.AddressNumber")]
        public string AddressNumber { get; set; }

        [Required]
        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.City")]
        public string City { get; set; }

        [Required]
        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.Province")]
        public string Province { get; set; }

        [Required]
        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.Country")]
        public string Country { get; set; }

        [Required]
        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.ZipCode")]
        public string ZipCode { get; set; }

        [Required]
        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.TaxCode")]
        public string TaxCode { get; set; }

        [Required]
        [LocalizedDisplay("Plugins.SmartStore.FE.Settings.VatCode")]
        public string VatCode { get; set; }
    }
}
