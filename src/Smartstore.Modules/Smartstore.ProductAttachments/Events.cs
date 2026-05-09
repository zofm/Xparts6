using System.Threading.Tasks;
using Smartstore.Events;
using Smartstore.Web.Rendering.Events;

namespace Smartstore.ProductAttachments
{
    internal class Events : IConsumer
    {
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
        }
    }
}
