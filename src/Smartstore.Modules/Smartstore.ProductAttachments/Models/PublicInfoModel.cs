namespace Smartstore.ProductAttachments.Models
{
    public class PublicInfoModel : ModelBase
    {
        public int ProductId { get; set; }

        public List<AttachmentItem> Attachments { get; set; } = new();

        public class AttachmentItem
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }
        }
    }
}
