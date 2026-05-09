using System.Threading.Tasks;
using Smartstore.Core.Widgets;
using Smartstore.Engine.Modularity;
using Smartstore.ProductAttachments.Components;

namespace Smartstore.ProductAttachments
{
    internal class Module : ModuleBase, IActivatableWidget
    {
        public Localizer T { get; set; } = NullLocalizer.Instance;

        public string[] GetWidgetZones()
            => ["productdetail_tabs_after"];

        public Widget GetDisplayWidget(string widgetZone, object model, int storeId)
            => new ComponentWidget(typeof(ProductAttachmentsViewComponent), model);

        public override async Task InstallAsync(ModuleInstallationContext context)
        {
            await ImportLanguageResourcesAsync();
            await base.InstallAsync(context);
        }

        public override async Task UninstallAsync()
        {
            await DeleteLanguageResourcesAsync();
            await base.UninstallAsync();
        }
    }
}
