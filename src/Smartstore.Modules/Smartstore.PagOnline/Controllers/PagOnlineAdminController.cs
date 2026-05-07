using Microsoft.AspNetCore.Mvc;
using Smartstore.ComponentModel;
using Smartstore.Core.Security;
using Smartstore.Engine.Modularity;
using Smartstore.PagOnline.Models;
using Smartstore.PagOnline.Providers;
using Smartstore.PagOnline.Settings;
using Smartstore.Web.Controllers;
using Smartstore.Web.Modelling.Settings;

namespace Smartstore.PagOnline.Controllers
{
    [Area("Admin")]
    public class PagOnlineAdminController : AdminController
    {
        [LoadSetting, AuthorizeAdmin]
        public IActionResult Configure(PagOnlineSettings settings)
        {
            var model = MiniMapper.Map<PagOnlineSettings, ConfigurationModel>(settings);
            return View(model);
        }

        [HttpPost, SaveSetting, AuthorizeAdmin]
        public IActionResult Configure(ConfigurationModel model, PagOnlineSettings settings)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }

            ModelState.Clear();
            MiniMapper.Map(model, settings);

            return RedirectToAction(nameof(Configure));
        }
    }
}
