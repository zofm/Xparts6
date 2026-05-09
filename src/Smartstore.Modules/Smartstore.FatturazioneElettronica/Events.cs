using Microsoft.AspNetCore.Http;
using Smartstore.Core.Identity;
using Smartstore.Events;
using Smartstore.FatturazioneElettronica.Models;
using Smartstore.Web.Rendering.Events;

namespace Smartstore.FatturazioneElettronica
{
    internal class Events : IConsumer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Events(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task HandleEventAsync(TabStripCreated message)
        {
            if (message.TabStripName == "order-edit")
            {
                var entityId = ((Smartstore.Web.Modelling.EntityModelBase)message.Model).Id;

                await message.TabFactory.AppendAsync(builder => builder
                    .Text("Fatturazione")
                    .Name("tab-fatturazione")
                    .Icon("fa fa-file-invoice fa-lg fa-fw")
                    .LinkHtmlAttributes(new { data_tab_name = "FatturazioneElettronica" })
                    .Action("OrderEditTab", "FatturazioneElettronicaAdmin", new { area = "Admin", entityId })
                    .Ajax());
            }
        }

        public async Task HandleEventAsync(CustomerRegisteredEvent message)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                return;
            }

            var sdiCode = request.Form["SdiCode"].ToString().Trim();
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
                    message.Customer.GenericAttributes.Set("SdiCode", sdiCode.ToUpperInvariant());
                    await message.Customer.GenericAttributes.SaveChangesAsync();
                }
            }
        }
    }
}
