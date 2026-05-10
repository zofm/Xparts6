global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
global using Microsoft.EntityFrameworkCore;
using Smartstore.Engine.Modularity;
using Smartstore.Http;
using Smartstore.MotoImporter.Settings;
using Smartstore.MotoImporter.Tasks;
using Smartstore.Scheduling;

namespace Smartstore.MotoImporter
{
    internal class Module : ModuleBase, IConfigurable
    {
        private readonly ITaskStore _taskStore;

        public Module(ITaskStore taskStore)
        {
            _taskStore = taskStore;
        }

        public RouteInfo GetConfigurationRoute()
            => new("Configure", "MotoImporterAdmin", new { area = "Admin" });

        public override async Task InstallAsync(ModuleInstallationContext context)
        {
            await SaveSettingsAsync<MotoImporterSettings>();
            await ImportLanguageResourcesAsync();

            await _taskStore.GetOrAddTaskAsync<ImportMotorbikeAttributesTask>(x =>
            {
                x.Name = "Import motorbike attributes from Excel";
                x.CronExpression = "0 3 * * *";
                x.Enabled = false;
            });

            await base.InstallAsync(context);
        }

        public override async Task UninstallAsync()
        {
            await DeleteSettingsAsync<MotoImporterSettings>();
            await DeleteLanguageResourcesAsync();
            await _taskStore.TryDeleteTaskAsync<ImportMotorbikeAttributesTask>();
            await base.UninstallAsync();
        }
    }
}
