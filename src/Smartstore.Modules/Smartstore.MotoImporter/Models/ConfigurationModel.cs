using Smartstore.Web.Modelling;

namespace Smartstore.MotoImporter.Models
{
    public class ConfigurationModel : ModelBase
    {
        [LocalizedDisplay("Plugins.Xparts.MotoImporter.ExcelFolderPath")]
        public string ExcelFolderPath { get; set; }
    }
}
