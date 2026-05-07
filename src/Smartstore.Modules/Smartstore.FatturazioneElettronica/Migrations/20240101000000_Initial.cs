using FluentMigrator;
using Smartstore.Core.Data.Migrations;
using Smartstore.FatturazioneElettronica.Domain;

namespace Smartstore.FatturazioneElettronica.Migrations
{
    [MigrationVersion("2024-01-01 00:00:00", "FatturazioneElettronica: Initial")]
    internal class Initial : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("FE_Invoice").Exists())
            {
                Create.Table("FE_Invoice")
                    .WithIdColumn()
                    .WithColumn(nameof(Invoice.OrderId)).AsInt32().NotNullable().Indexed()
                    .WithColumn(nameof(Invoice.Year)).AsInt32().NotNullable()
                    .WithColumn(nameof(Invoice.Number)).AsInt32().NotNullable()
                    .WithColumn(nameof(Invoice.ExemptionId)).AsInt32().Nullable()
                    .WithColumn(nameof(Invoice.CreatedOnUtc)).AsDateTime2().Nullable()
                    .WithColumn(nameof(Invoice.UpdatedOnUtc)).AsDateTime2().Nullable()
                    .WithColumn(nameof(Invoice.HasXmlFile)).AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn(nameof(Invoice.Causal)).AsString(2000).Nullable();
            }

            if (!Schema.Table("FE_InvoiceHistory").Exists())
            {
                Create.Table("FE_InvoiceHistory")
                    .WithIdColumn()
                    .WithColumn(nameof(InvoiceHistory.InvoiceId)).AsInt32().NotNullable().Indexed()
                    .WithColumn(nameof(InvoiceHistory.Status)).AsInt32().NotNullable()
                    .WithColumn(nameof(InvoiceHistory.SdiFileName)).AsString(500).Nullable()
                    .WithColumn(nameof(InvoiceHistory.ErrorCode)).AsString(100).Nullable()
                    .WithColumn(nameof(InvoiceHistory.ErrorDescription)).AsString(2000).Nullable()
                    .WithColumn(nameof(InvoiceHistory.CreatedOnUtc)).AsDateTime2().NotNullable();

                Create.ForeignKey()
                    .FromTable("FE_InvoiceHistory").ForeignColumn(nameof(InvoiceHistory.InvoiceId))
                    .ToTable("FE_Invoice").PrimaryColumn("Id");
            }
        }

        public override void Down()
        {
            // INFO: no down initial migration.
        }
    }
}
