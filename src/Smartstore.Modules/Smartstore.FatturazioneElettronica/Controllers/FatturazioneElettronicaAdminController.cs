using Microsoft.AspNetCore.Mvc;
using Smartstore.ComponentModel;
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

        public FatturazioneElettronicaAdminController(IFatturazioneService fatturazioneService)
        {
            _fatturazioneService = fatturazioneService;
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
