using Microsoft.AspNetCore.Mvc;
using Smartstore.Web.Components;

namespace Smartstore.PagOnline.Components
{
    public class PagOnlinePaymentInfoViewComponent : SmartViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
