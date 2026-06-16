using Microsoft.AspNetCore.Mvc;
using Smartstore.ComponentModel;
using Smartstore.Core.Data;
using Smartstore.Core.Identity;
using Smartstore.Core.Security;
using Smartstore.FatturazioneElettronica.Models;
using Smartstore.FatturazioneElettronica.Services;
using Smartstore.FatturazioneElettronica.Settings;
using Smartstore.Web.Controllers;
using Smartstore.Web.Modelling.Settings;

namespace Smartstore.FatturazioneElettronica.Controllers
{
    [Area("Admin")]
    public class FatturazioneElettronicaAdminController : AdminController
    {
        private readonly IFatturazioneService _fatturazioneService;
        private readonly SmartDbContext _db;

        public FatturazioneElettronicaAdminController(
            IFatturazioneService fatturazioneService,
            SmartDbContext db)
        {
            _fatturazioneService = fatturazioneService;
            _db = db;
        }

        [LoadSetting, AuthorizeAdmin]
        public IActionResult Configure(FatturazioneElettronicaSettings settings)
        {
            var model = MiniMapper.Map<FatturazioneElettronicaSettings, ConfigurationModel>(settings);
            return View(model);
        }

        [HttpPost, SaveSetting, AuthorizeAdmin]
        public IActionResult Configure(ConfigurationModel model, FatturazioneElettronicaSettings settings)
        {
            if (!ModelState.IsValid)
                return Configure(settings);

            ModelState.Clear();
            MiniMapper.Map(model, settings);
            return RedirectToAction(nameof(Configure));
        }

        [AuthorizeAdmin]
        public async Task<IActionResult> CustomerEditTab(int customerId)
        {
            var customer = await _db.Customers.FindByIdAsync(customerId, false);
            if (customer == null)
                return NotFound();

            var sdiCode = customer.GenericAttributes.Get<string>("SdiCode");
            var model = new CustomerSdiCodeModel
            {
                CustomerId = customerId,
                SdiCode = sdiCode
            };

            return PartialView(model);
        }

        [AuthorizeAdmin, HttpPost]
        public async Task<IActionResult> SaveCustomerSdiCode(int customerId, string sdiCode)
        {
            var customer = await _db.Customers.FindByIdAsync(customerId);
            if (customer == null)
                return NotFound();

            // Clean and validate SDI code
            sdiCode = sdiCode?.Trim();
            if (sdiCode.HasValue())
            {
                // SDI code: exactly 7 alphanumeric characters (strip invalid chars, truncate to 7)
                sdiCode = new string(sdiCode.Where(char.IsAsciiLetterOrDigit).ToArray());
                if (sdiCode.Length > 7)
                {
                    sdiCode = sdiCode[..7];
                }

                if (sdiCode.Length == 7)
                {
                    customer.GenericAttributes.Set("SdiCode", sdiCode.ToUpperInvariant());
                }
                else
                {
                    customer.GenericAttributes.Set("SdiCode", string.Empty);
                }
            }
            else
            {
                customer.GenericAttributes.Set("SdiCode", string.Empty);
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        [AuthorizeAdmin]
        public IActionResult OrderEditTab(int entityId, string entityName)
        {
            var model = new OrderEditTabModel { EntityId = entityId };
            model.Bind(_fatturazioneService);
            return PartialView(model);
        }

        [AuthorizeAdmin, HttpPost]
        public IActionResult CreateInvoice(OrderEditTabModel model)
        {
            int? exemptId = model.SelectedExemptId > -1 ? model.SelectedExemptId : (int?)null;
            _fatturazioneService.CreateInvoice(model.EntityId, exemptId, model.Causal);
            return Json(true);
        }

        [AuthorizeAdmin, HttpPost]
        public IActionResult RecreateInvoice(OrderEditTabModel model)
        {
            if (!model.InvoiceId.HasValue)
                return Json(false);

            int? exemptId = model.SelectedExemptId > -1 ? model.SelectedExemptId : (int?)null;
            _fatturazioneService.RecreateInvoice(model.EntityId, model.InvoiceId.Value, exemptId, model.Causal);
            return Json(true);
        }

        [AuthorizeAdmin, HttpPost]
        public IActionResult DeleteInvoice(int orderId)
        {
            if (_fatturazioneService.CanInvoiceBeDeleted(orderId))
                _fatturazioneService.DeleteInvoiceByOrderId(orderId);

            return Json(true);
        }

        public IActionResult InvoicePdf(int orderId)
        {
            var pdfFileName = $"W{orderId}.pdf";
            var folder = new DirectoryInfo(Path.Combine(_fatturazioneService.GetInvoiceByOrderId(orderId) != null
                ? Directory.GetCurrentDirectory()
                : throw new InvalidOperationException("Invoice not found")));

            return NotFound();
        }
    }
}
