using Microsoft.AspNetCore.Mvc;
using Smartstore.ComponentModel;
using Smartstore.Core.Security;
using Smartstore.MotoImporter.Models;
using Smartstore.MotoImporter.Settings;
using Smartstore.Web.Controllers;
using Smartstore.Web.Modelling.Settings;

namespace Smartstore.MotoImporter.Controllers
{
    [Area("Admin")]
    public class MotoImporterAdminController : AdminController
    {
        [LoadSetting, AuthorizeAdmin]
        public IActionResult Configure(MotoImporterSettings settings)
        {
            var model = MiniMapper.Map<MotoImporterSettings, ConfigurationModel>(settings);
            return View(model);
        }

        [HttpPost, SaveSetting, AuthorizeAdmin]
        public IActionResult Configure(ConfigurationModel model, MotoImporterSettings settings)
        {
            if (!ModelState.IsValid)
                return Configure(settings);

            ModelState.Clear();
            MiniMapper.Map(model, settings);
            return RedirectToAction(nameof(Configure));
        }
    }
}
