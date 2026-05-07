namespace Smartstore.PagOnline.Models
{
    [LocalizedDisplay("Plugins.Smartstore.PagOnline.")]
    public class ConfigurationModel : ModelBase
    {
        [LocalizedDisplay("*Tid")]
        public string Tid { get; set; }

        [LocalizedDisplay("*KSig")]
        public string Ksig { get; set; }

        [LocalizedDisplay("*UseSandbox")]
        public bool UseSandbox { get; set; }

        [LocalizedDisplay("Admin.Configuration.Payment.Methods.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [LocalizedDisplay("Admin.Configuration.Payment.Methods.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
    }
}
