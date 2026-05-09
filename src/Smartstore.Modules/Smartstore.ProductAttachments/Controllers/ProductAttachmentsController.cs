using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Content.Media;
using Smartstore.Core.Data;
using Smartstore.ProductAttachments.Domain;
using Smartstore.ProductAttachments.Services;
using Smartstore.Web.Controllers;

namespace Smartstore.ProductAttachments.Controllers
{
    public class ProductAttachmentsController : SmartController
    {
        private readonly SmartDbContext _db;
        private readonly IProductAttachmentsService _service;
        private readonly IMediaService _mediaService;

        public ProductAttachmentsController(
            SmartDbContext db,
            IProductAttachmentsService service,
            IMediaService mediaService)
        {
            _db = db;
            _service = service;
            _mediaService = mediaService;
        }

        [Route("productattachments/download/{id:int}")]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var download = await _db.Downloads
                .Where(x => x.EntityId == id && x.EntityName == nameof(ProductAttachment))
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (download == null)
                return NotFound();

            if (download.MediaFileId.HasValue)
            {
                var file = await _mediaService.GetFileByIdAsync(download.MediaFileId.Value);
                if (file != null)
                    return Redirect(file.GetUrl());
            }
            else if (download.DownloadUrl.HasValue())
            {
                return Redirect(download.DownloadUrl);
            }

            return NotFound();
        }
    }
}
