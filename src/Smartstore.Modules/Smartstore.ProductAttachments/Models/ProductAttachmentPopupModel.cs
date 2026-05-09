using System.ComponentModel.DataAnnotations;
using Smartstore.Core.Content.Media;

namespace Smartstore.ProductAttachments.Models
{
    public class ProductAttachmentPopupModel : TabbableModel, ILocalizedModel<ProductAttachmentPopupLocalizedModel>
    {
        public ProductAttachmentPopupModel()
        {
            Locales = new List<ProductAttachmentPopupLocalizedModel>();
        }

        public int ProductId { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.ProductAttachments.Name")]
        public string Name { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.ProductAttachments.Description")]
        public string Description { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.ProductAttachments.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.ProductAttachments.IsActive")]
        public bool IsActive { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.ProductAttachments.Download")]
        [UIHint("Download")]
        public int? DownloadId { get; set; }

        public string DownloadThumbUrl { get; set; }

        public Download CurrentDownload { get; set; }

        public List<ProductAttachmentPopupLocalizedModel> Locales { get; set; }

        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class ProductAttachmentPopupLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.ProductAttachments.Name")]
        public string Name { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.ProductAttachments.Description")]
        public string Description { get; set; }
    }
}
