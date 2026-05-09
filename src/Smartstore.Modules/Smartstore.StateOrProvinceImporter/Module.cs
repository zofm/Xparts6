using System.Threading.Tasks;
using Smartstore.Engine.Modularity;
using Smartstore.Http;
using Smartstore.Scheduling;
using Smartstore.StateOrProvinceImporter.Settings;
using Smartstore.StateOrProvinceImporter.Tasks;

namespace Smartstore.StateOrProvinceImporter
{
    internal class Module : ModuleBase, IConfigurable
    {
        private readonly ITaskStore _taskStore;

        public Module(ITaskStore taskStore)
        {
            _taskStore = taskStore;
        }

        public RouteInfo GetConfigurationRoute()
            => new("Configure", "StateOrProvinceImporterAdmin", new { area = "Admin" });

        public override async Task InstallAsync(ModuleInstallationContext context)
        {
            await SaveSettingsAsync<StateOrProvinceImporterSettings>();
            await ImportLanguageResourcesAsync();

            await _taskStore.GetOrAddTaskAsync<StateOrProvinceImporterTask>(x =>
            {
                x.Name = "Import states/provinces from CSV";
                x.CronExpression = "0 4 * * *";
                x.Enabled = false;
            });

            await base.InstallAsync(context);
        }

        public override async Task UninstallAsync()
        {
            await DeleteSettingsAsync<StateOrProvinceImporterSettings>();
            await DeleteLanguageResourcesAsync();
            await _taskStore.TryDeleteTaskAsync<StateOrProvinceImporterTask>();
            await base.UninstallAsync();
        }
    }
}
