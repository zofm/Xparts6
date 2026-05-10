using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Content.Media;
using Smartstore.Core.Data;
using Smartstore.Core.Localization;
using Smartstore.Core.Security;
using Smartstore.ProductAttachments.Domain;
using Smartstore.ProductAttachments.Models;
using Smartstore.ProductAttachments.Services;
using Smartstore.Web.Controllers;

namespace Smartstore.ProductAttachments.Controllers
{
    [Area("Admin")]
    public class ProductAttachmentsAdminController : AdminController
    {
        private readonly SmartDbContext _db;
        private readonly IProductAttachmentsService _service;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IMediaService _mediaService;

        public ProductAttachmentsAdminController(
            SmartDbContext db,
            IProductAttachmentsService service,
            ILocalizedEntityService localizedEntityService,
            IMediaService mediaService)
        {
            _db = db;
            _service = service;
            _localizedEntityService = localizedEntityService;
            _mediaService = mediaService;
        }

        [AuthorizeAdmin]
        public IActionResult AdminEditTab(int productId)
        {
            var model = new ProductAttachmentsGridModel
            {
                EntityId = productId,
                EntityName = nameof(ProductAttachment)
            };
            return PartialView(model);
        }

        [AuthorizeAdmin]
        [HttpGet]
        public async Task<IActionResult> List(int productId, int pageIndex = 0, int pageSize = 50)
        {
            var (items, total) = await _service.GetPagedProductAttachmentsAsync(productId, pageIndex, pageSize);

            var gridItems = items.Select(x => new ProductAttachmentGridItemModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            }).ToList();

            return Json(new { Data = gridItems, Total = total });
        }

        [AuthorizeAdmin]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public async Task<IActionResult> CreatePopup(int productId, string btnId, string formId)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            var model = new ProductAttachmentPopupModel { ProductId = productId };
            await AddLocalesAsync<ProductAttachmentPopupLocalizedModel>(model.Locales, (locale, languageId) => Task.CompletedTask);

            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            ViewBag.IsEdit = false;

            return View(model);
        }

        [AuthorizeAdmin]
        [HttpPost]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public async Task<IActionResult> CreatePopup(ProductAttachmentPopupModel model, string btnId, string formId)
        {
            if (!model.DownloadId.HasValue || model.DownloadId == 0)
            {
                ModelState.AddModelError(nameof(model.DownloadId), T("Plugins.SmartStore.ProductAttachments.SelectFileToContinue"));
            }

            if (ModelState.IsValid)
            {
                var attachment = new ProductAttachment
                {
                    ProductId = model.ProductId,
                    Name = model.Name,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive
                };

                await _service.InsertAsync(attachment);
                await UpdateLocalesAsync(attachment, model);
                await UpdateDownloadAsync(model.DownloadId, attachment.Id);

                ViewBag.RefreshPage = true;
            }

            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            ViewBag.IsEdit = false;

            return View(model);
        }

        [AuthorizeAdmin]
        [Permission(Permissions.Catalog.Product.Read)]
        public async Task<IActionResult> EditPopup(int id, string btnId, string formId)
        {
            var attachment = await _service.GetByIdAsync(id);
            if (attachment == null)
                return NotFound();

            var model = new ProductAttachmentPopupModel
            {
                Id = attachment.Id,
                ProductId = attachment.ProductId,
                Name = attachment.Name,
                Description = attachment.Description,
                DisplayOrder = attachment.DisplayOrder,
                IsActive = attachment.IsActive
            };

            await AddLocalesAsync(model.Locales, async (locale, languageId) =>
            {
                locale.Name = attachment.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = attachment.GetLocalized(x => x.Description, languageId, false, false);
            });

            // Resolve current download for this attachment entity
            var download = await _db.Downloads
                .Include(x => x.MediaFile)
                .Where(x => x.EntityId == id && x.EntityName == nameof(ProductAttachment))
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (download != null)
            {
                model.DownloadId = download.Id;
                model.CurrentDownload = download;
                if (download.MediaFileId.HasValue)
                {
                    var file = await _mediaService.GetFileByIdAsync(download.MediaFileId.Value);
                    model.DownloadThumbUrl = file?.GetUrl();
                }
            }

            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            ViewBag.IsEdit = true;

            return View(model);
        }

        [AuthorizeAdmin]
        [HttpPost]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public async Task<IActionResult> EditPopup(ProductAttachmentPopupModel model, string btnId, string formId)
        {
            var attachment = await _service.GetByIdAsync(model.Id);
            if (attachment == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                attachment.Name = model.Name;
                attachment.Description = model.Description;
                attachment.IsActive = model.IsActive;
                attachment.DisplayOrder = model.DisplayOrder;

                await _service.UpdateAsync(attachment);
                await UpdateLocalesAsync(attachment, model);

                if (model.DownloadId.HasValue && model.DownloadId > 0)
                {
                    await UpdateDownloadAsync(model.DownloadId, attachment.Id);
                }

                ViewBag.RefreshPage = true;
            }

            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            ViewBag.IsEdit = true;

            return View(model);
        }

        [AuthorizeAdmin]
        [HttpPost]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public async Task<IActionResult> Delete(int id)
        {
            var attachment = await _service.GetByIdAsync(id);
            if (attachment == null)
                return NotFound();

            await _service.DeleteAsync(attachment);

            return Json(new { success = true });
        }

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

        private async Task UpdateLocalesAsync(ProductAttachment attachment, ProductAttachmentPopupModel model)
        {
            foreach (var locale in model.Locales)
            {
                await _localizedEntityService.ApplyLocalizedValueAsync(attachment, x => x.Name, locale.Name, locale.LanguageId);
                await _localizedEntityService.ApplyLocalizedValueAsync(attachment, x => x.Description, locale.Description, locale.LanguageId);
            }

            await _db.SaveChangesAsync();
        }

        private async Task UpdateDownloadAsync(int? downloadId, int attachmentId)
        {
            if (!downloadId.HasValue || downloadId == 0)
                return;

            var download = await _db.Downloads.FindAsync(downloadId.Value);
            if (download != null)
            {
                // Existing Download record (e.g. created via URL save): just attach it to the entity.
                download.IsTransient = false;
                download.EntityId = attachmentId;
                download.EntityName = nameof(ProductAttachment);
                await _db.SaveChangesAsync();
            }
            else
            {
                // No Download record found: the id is a MediaFileId coming from a direct file upload.
                var mediaFile = await _db.MediaFiles.FindAsync(downloadId.Value);
                if (mediaFile != null)
                {
                    var newDownload = new Smartstore.Core.Content.Media.Download
                    {
                        MediaFileId = mediaFile.Id,
                        EntityId = attachmentId,
                        EntityName = nameof(ProductAttachment),
                        DownloadGuid = Guid.NewGuid(),
                        UseDownloadUrl = false,
                        DownloadUrl = string.Empty,
                        IsTransient = false,
                        UpdatedOnUtc = DateTime.UtcNow
                    };
                    _db.Downloads.Add(newDownload);
                    await _db.SaveChangesAsync();
                }
            }
        }
    }
}
