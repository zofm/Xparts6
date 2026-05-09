using System.Collections.Generic;
using System.Threading.Tasks;
using Smartstore.ProductAttachments.Domain;

namespace Smartstore.ProductAttachments.Services
{
    public interface IProductAttachmentsService
    {
        Task<ProductAttachment> GetByIdAsync(int id);

        Task InsertAsync(ProductAttachment record);

        Task UpdateAsync(ProductAttachment record);

        Task DeleteAsync(ProductAttachment record);

        Task<List<ProductAttachment>> GetProductAttachmentsAsync(int productId);

        Task<(List<ProductAttachment> Items, int TotalCount)> GetPagedProductAttachmentsAsync(int productId, int pageIndex, int pageSize);
    }
}
