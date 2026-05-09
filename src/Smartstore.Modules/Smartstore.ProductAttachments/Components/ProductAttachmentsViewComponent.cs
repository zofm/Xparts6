using Microsoft.AspNetCore.Mvc;
using Smartstore.ProductAttachments.Models;
using Smartstore.ProductAttachments.Services;
using Smartstore.Web.Components;
using Smartstore.Web.Models.Catalog;

namespace Smartstore.ProductAttachments.Components
{
    public class ProductAttachmentsViewComponent : SmartViewComponent
    {
        private readonly IProductAttachmentsService _service;

        public ProductAttachmentsViewComponent(IProductAttachmentsService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(object model)
        {
            if (model is not ProductDetailsModel productModel)
                return Empty();

            var attachments = await _service.GetProductAttachmentsAsync(productModel.Id);
            if (attachments.Count == 0)
                return Empty();

            var viewModel = new PublicInfoModel
            {
                ProductId = productModel.Id,
                Attachments = attachments.Select(x => new PublicInfoModel.AttachmentItem
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
