using Smartstore.Core.Configuration;

namespace Smartstore.MotoImporter.Settings
{
    public class MotoImporterSettings : ISettings
    {
        /// <summary>
        /// Full path to the folder containing Excel files to import.
        /// Each .xlsx file in this folder will be processed and then deleted.
        /// </summary>
        public string ExcelFolderPath { get; set; } = string.Empty;
    }
}
