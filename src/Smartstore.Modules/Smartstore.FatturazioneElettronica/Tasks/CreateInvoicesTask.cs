using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Smartstore.FatturazioneElettronica.Services;
using Smartstore.Scheduling;

namespace Smartstore.FatturazioneElettronica.Tasks
{
    public class CreateInvoicesTask : ITask
    {
        private readonly IFatturazioneService _fatturazioneService;

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public CreateInvoicesTask(IFatturazioneService fatturazioneService)
        {
            _fatturazioneService = fatturazioneService;
        }

        public Task Run(TaskExecutionContext ctx, CancellationToken cancelToken = default)
        {
            _fatturazioneService.NormalizeAddresses();

            var orders = _fatturazioneService.GetAllInvoicesToCreateXml().Select(x => x.OrderId).ToList();

            foreach (var orderId in orders)
            {
                try
                {
                    var file = _fatturazioneService.CreateInvoiceXml(orderId);
                    if (file != null)
                        Logger.LogInformation("Fattura creata per l'ordine n. {OrderId}.", orderId);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Impossibile creare file XML della fattura per l'ordine n. {OrderId}.", orderId);
                }
            }

            return Task.CompletedTask;
        }
    }
}
