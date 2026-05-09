using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Data;
using Smartstore.ProductAttachments.Domain;

namespace Smartstore.ProductAttachments.Services
{
    public class ProductAttachmentsService : IProductAttachmentsService
    {
        private readonly SmartDbContext _db;

        public ProductAttachmentsService(SmartDbContext db)
        {
            _db = db;
        }

        public Task<ProductAttachment> GetByIdAsync(int id)
            => _db.Set<ProductAttachment>().FindAsync(id).AsTask();

        public async Task InsertAsync(ProductAttachment record)
        {
            Guard.NotNull(record);
            record.CreatedOnUtc = DateTime.UtcNow;
            _db.Set<ProductAttachment>().Add(record);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductAttachment record)
        {
            Guard.NotNull(record);
            record.UpdatedOnUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(ProductAttachment record)
        {
            Guard.NotNull(record);
            _db.Set<ProductAttachment>().Remove(record);
            await _db.SaveChangesAsync();
        }

        public Task<List<ProductAttachment>> GetProductAttachmentsAsync(int productId)
        {
            return _db.Set<ProductAttachment>()
                .Where(x => x.ProductId == productId && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<(List<ProductAttachment> Items, int TotalCount)> GetPagedProductAttachmentsAsync(int productId, int pageIndex, int pageSize)
        {
            var query = _db.Set<ProductAttachment>()
                .Where(x => x.ProductId == productId)
                .OrderBy(x => x.DisplayOrder);

            var total = await query.CountAsync();
            var items = await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

            return (items, total);
        }
    }
}
