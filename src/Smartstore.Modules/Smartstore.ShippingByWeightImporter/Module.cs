global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
global using Microsoft.EntityFrameworkCore;
using Smartstore.Engine.Modularity;
using Smartstore.Http;
using Smartstore.Scheduling;
using Smartstore.ShippingByWeightImporter.Settings;
using Smartstore.ShippingByWeightImporter.Tasks;

namespace Smartstore.ShippingByWeightImporter
{
    internal class Module : ModuleBase, IConfigurable
    {
        private readonly ITaskStore _taskStore;

        public Module(ITaskStore taskStore)
        {
            _taskStore = taskStore;
        }

        public RouteInfo GetConfigurationRoute()
            => new("Configure", "ShippingByWeightImporterAdmin", new { area = "Admin" });

        public override async Task InstallAsync(ModuleInstallationContext context)
        {
            await SaveSettingsAsync<ShippingByWeightImporterSettings>();
            await ImportLanguageResourcesAsync();

            await _taskStore.GetOrAddTaskAsync<ShippingByWeightImporterTask>(x =>
            {
                x.Name = "Import shipping costs from Excel";
                x.CronExpression = "0 3 * * *";
                x.Enabled = false;
            });

            await base.InstallAsync(context);
        }

        public override async Task UninstallAsync()
        {
            await DeleteSettingsAsync<ShippingByWeightImporterSettings>();
            await DeleteLanguageResourcesAsync();
            await _taskStore.TryDeleteTaskAsync<ShippingByWeightImporterTask>();
            await base.UninstallAsync();
        }
    }
}
