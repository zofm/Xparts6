using Microsoft.AspNetCore.Mvc;
using Smartstore.ComponentModel;
using Smartstore.Core.Security;
using Smartstore.StateOrProvinceImporter.Models;
using Smartstore.StateOrProvinceImporter.Settings;
using Smartstore.Web.Controllers;
using Smartstore.Web.Modelling.Settings;

namespace Smartstore.StateOrProvinceImporter.Controllers
{
    [Area("Admin")]
    public class StateOrProvinceImporterAdminController : AdminController
    {
        [LoadSetting, AuthorizeAdmin]
        public IActionResult Configure(StateOrProvinceImporterSettings settings)
        {
            var model = MiniMapper.Map<StateOrProvinceImporterSettings, ConfigurationModel>(settings);
            return View(model);
        }

        [HttpPost, SaveSetting, AuthorizeAdmin]
        public IActionResult Configure(ConfigurationModel model, StateOrProvinceImporterSettings settings)
        {
            if (!ModelState.IsValid)
                return Configure(settings);

            ModelState.Clear();
            MiniMapper.Map(model, settings);
            return RedirectToAction(nameof(Configure));
        }
    }
}
