using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smartstore.Core.Localization;
using Smartstore.Domain;

namespace Smartstore.ProductAttachments.Domain
{
    internal class ProductAttachmentMap : IEntityTypeConfiguration<ProductAttachment>
    {
        public void Configure(EntityTypeBuilder<ProductAttachment> builder)
        {
            builder.ToTable("ProductAttachment");
            builder.HasIndex(x => x.ProductId);
        }
    }

    /// <summary>
    /// Represents a downloadable file attachment associated with a product.
    /// </summary>
    public class ProductAttachment : BaseEntity, ILocalizedEntity
    {
        public int ProductId { get; set; }

        [StringLength(300)]
        public string Name { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreatedOnUtc { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }
    }
}
