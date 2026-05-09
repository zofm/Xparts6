using Smartstore.Web.Modelling;

namespace Smartstore.StateOrProvinceImporter.Models
{
    public class ConfigurationModel : ModelBase
    {
        [LocalizedDisplay("Plugins.SmartStore.StateOrProvinceImporter.CsvFilePath")]
        public string CsvFilePath { get; set; }
    }
}
