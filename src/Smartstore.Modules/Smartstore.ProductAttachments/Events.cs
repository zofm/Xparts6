using System.Threading.Tasks;
using Smartstore.Core.Localization;
using Smartstore.Core.Widgets;
using Smartstore.Events;
using Smartstore.ProductAttachments.Components;
using Smartstore.Web.Rendering.Events;

namespace Smartstore.ProductAttachments
{
    internal class Events : IConsumer
    {
        private readonly IText _text;

        public Events(IText text)
        {
            _text = text;
        }

        public async Task HandleEventAsync(TabStripCreated message)
        {
            if (message.TabStripName == "product-edit")
            {
                var productId = ((Smartstore.Web.Modelling.EntityModelBase)message.Model).Id;
                if (productId == 0)
                    return;

                await message.TabFactory.AppendAsync(builder => builder
                    .Text("Attachments")
                    .Name("tab-product-attachments")
                    .Icon("fa fa-paperclip fa-lg fa-fw")
                    .LinkHtmlAttributes(new { data_tab_name = "ProductAttachments" })
                    .Action("AdminEditTab", "ProductAttachmentsAdmin", new { area = "Admin", productId })
                    .Ajax());
            }
            else if (message.TabStripName == "pd-tabs")
            {
                await message.TabFactory.AppendAsync(builder => builder
                    .Text(_text.Get("Plugins.SmartStore.ProductAttachments.TabTitle"))
                    .Name("pd-attachments")
                    .Icon("paperclip", "bi")
                    .Content(new ComponentWidget(typeof(ProductAttachmentsViewComponent), message.Model))
                    .Ajax(false));
            }
        }
    }
}
