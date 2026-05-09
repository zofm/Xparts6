using Microsoft.AspNetCore.Mvc;
using Smartstore.ComponentModel;
using Smartstore.Core.Security;
using Smartstore.ShippingByWeightImporter.Models;
using Smartstore.ShippingByWeightImporter.Settings;
using Smartstore.Web.Controllers;
using Smartstore.Web.Modelling.Settings;

namespace Smartstore.ShippingByWeightImporter.Controllers
{
    [Area("Admin")]
    public class ShippingByWeightImporterAdminController : AdminController
    {
        [LoadSetting, AuthorizeAdmin]
        public IActionResult Configure(ShippingByWeightImporterSettings settings)
        {
            var model = MiniMapper.Map<ShippingByWeightImporterSettings, ConfigurationModel>(settings);
            return View(model);
        }

        [HttpPost, SaveSetting, AuthorizeAdmin]
        public IActionResult Configure(ConfigurationModel model, ShippingByWeightImporterSettings settings)
        {
            if (!ModelState.IsValid)
                return Configure(settings);

            ModelState.Clear();
            MiniMapper.Map(model, settings);
            return RedirectToAction(nameof(Configure));
        }
    }
}
