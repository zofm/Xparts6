using FluentMigrator;
using Smartstore.Core.Data.Migrations;
using Smartstore.ProductAttachments.Domain;

namespace Smartstore.ProductAttachments.Migrations
{
    [MigrationVersion("2022-06-01 13:45:05", "ProductAttachments: Initial")]
    internal class Initial : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("ProductAttachment").Exists())
            {
                Create.Table("ProductAttachment")
                    .WithIdColumn()
                    .WithColumn(nameof(ProductAttachment.ProductId)).AsInt32().NotNullable().Indexed()
                    .WithColumn(nameof(ProductAttachment.Name)).AsString(300).Nullable()
                    .WithColumn(nameof(ProductAttachment.Description)).AsString(4000).Nullable()
                    .WithColumn(nameof(ProductAttachment.DisplayOrder)).AsInt32().NotNullable().WithDefaultValue(0)
                    .WithColumn(nameof(ProductAttachment.IsActive)).AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn(nameof(ProductAttachment.CreatedOnUtc)).AsDateTime2().Nullable()
                    .WithColumn(nameof(ProductAttachment.UpdatedOnUtc)).AsDateTime2().Nullable();
            }
        }

        public override void Down()
        {
            // INFO: no down initial migration.
        }
    }
}
