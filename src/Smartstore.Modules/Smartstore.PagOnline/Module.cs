using Smartstore.Engine.Modularity;
using Smartstore.Http;
using Smartstore.PagOnline.Settings;

namespace Smartstore.PagOnline
{
    internal class Module : ModuleBase, IConfigurable
    {
        public RouteInfo GetConfigurationRoute()
            => new("Configure", "PagOnlineAdmin", new { area = "Admin" });

        public override async Task InstallAsync(ModuleInstallationContext context)
        {
            await SaveSettingsAsync<PagOnlineSettings>();
            await ImportLanguageResourcesAsync();
            await base.InstallAsync(context);
        }

        public override async Task UninstallAsync()
        {
            await DeleteSettingsAsync<PagOnlineSettings>();
            await DeleteLanguageResourcesAsync();
            await base.UninstallAsync();
        }
    }
}
