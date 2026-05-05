namespace Smartstore.PagOnline.Models
{
    public class ConfigurationModel : ModelBase
    {
        [LocalizedDisplay("Plugins.Smartstore.PagOnline.Tid")]
        public string Tid { get; set; }

        [LocalizedDisplay("Plugins.Smartstore.PagOnline.KSig")]
        public string Ksig { get; set; }

        [LocalizedDisplay("Plugins.Smartstore.PagOnline.UseSandbox")]
        public bool UseSandbox { get; set; }

        [LocalizedDisplay("Admin.Configuration.Payment.Methods.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [LocalizedDisplay("Admin.Configuration.Payment.Methods.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
    }
}
