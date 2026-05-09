namespace Smartstore.ProductAttachments.Models
{
    public class ProductAttachmentsGridModel : ModelBase
    {
        public int EntityId { get; set; }

        public string EntityName { get; set; }

        public int GridPageSize { get; set; } = 50;
    }

    public class ProductAttachmentGridItemModel : EntityModelBase
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
