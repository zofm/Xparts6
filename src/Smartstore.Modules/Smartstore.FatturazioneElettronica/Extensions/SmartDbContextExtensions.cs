using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Data;
using Smartstore.FatturazioneElettronica.Domain;

namespace Smartstore.FatturazioneElettronica
{
    public static class SmartDbContextExtensions
    {
        public static DbSet<Invoice> FEInvoices(this SmartDbContext db)
            => db.Set<Invoice>();

        public static DbSet<InvoiceHistory> FEInvoiceHistories(this SmartDbContext db)
            => db.Set<InvoiceHistory>();
    }
}
