using Smartstore.Engine.Modularity;
using Smartstore.FatturazioneElettronica.Settings;
using Smartstore.FatturazioneElettronica.Tasks;
using Smartstore.Http;
using Smartstore.Scheduling;

namespace Smartstore.FatturazioneElettronica
{
    internal class Module : ModuleBase, IConfigurable
    {
        private readonly ITaskStore _taskStore;

        public Module(ITaskStore taskStore)
        {
            _taskStore = taskStore;
        }

        public RouteInfo GetConfigurationRoute()
            => new("Configure", "FatturazioneElettronicaAdmin", new { area = "Admin" });

        public override async Task InstallAsync(ModuleInstallationContext context)
        {
            await SaveSettingsAsync<FatturazioneElettronicaSettings>();
            await ImportLanguageResourcesAsync();

            await _taskStore.GetOrAddTaskAsync<UploadInvoicesToSdiTask>(x =>
            {
                x.Name = "Upload invoices to SdI";
                x.CronExpression = "*/30 * * * *";
                x.Enabled = false;
            });

            await _taskStore.GetOrAddTaskAsync<CheckInvoicesStatusTask>(x =>
            {
                x.Name = "Check invoices from SdI";
                x.CronExpression = "*/10 * * * *";
                x.Enabled = false;
            });

            await _taskStore.GetOrAddTaskAsync<CreateInvoicesTask>(x =>
            {
                x.Name = "Create XML files for invoices";
                x.CronExpression = "*/15 * * * *";
                x.Enabled = false;
            });

            await base.InstallAsync(context);
        }

        public override async Task UninstallAsync()
        {
            await DeleteSettingsAsync<FatturazioneElettronicaSettings>();
            await DeleteLanguageResourcesAsync();

            await _taskStore.TryDeleteTaskAsync<UploadInvoicesToSdiTask>();
            await _taskStore.TryDeleteTaskAsync<CheckInvoicesStatusTask>();
            await _taskStore.TryDeleteTaskAsync<CreateInvoicesTask>();

            await base.UninstallAsync();
        }
    }
}
