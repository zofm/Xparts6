using Smartstore.Web.Modelling;

namespace Smartstore.ShippingByWeightImporter.Models
{
    public class ConfigurationModel : ModelBase
    {
        [LocalizedDisplay("Plugins.SmartStore.ShippingByWeightImporter.ExcelFilePath")]
        public string ExcelFilePath { get; set; }
    }
}
