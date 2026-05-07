using Smartstore.Events;
using Smartstore.FatturazioneElettronica.Models;
using Smartstore.Web.Rendering.Events;

namespace Smartstore.FatturazioneElettronica
{
    internal class Events : IConsumer
    {
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
    }
}
