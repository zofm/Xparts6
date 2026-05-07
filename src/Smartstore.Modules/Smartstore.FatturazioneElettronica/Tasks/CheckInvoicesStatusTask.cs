using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Smartstore.FatturazioneElettronica.Domain;
using Smartstore.FatturazioneElettronica.Providers.Aruba;
using Smartstore.FatturazioneElettronica.Providers.Models;
using Smartstore.FatturazioneElettronica.Services;
using Smartstore.FatturazioneElettronica.Settings;
using Smartstore.Scheduling;

namespace Smartstore.FatturazioneElettronica.Tasks
{
    public class CheckInvoicesStatusTask : ITask
    {
        private readonly IFatturazioneService _fatturazioneService;
        private readonly FatturazioneElettronicaSettings _fatturazioneSettings;

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public CheckInvoicesStatusTask(
            IFatturazioneService fatturazioneService,
            FatturazioneElettronicaSettings fatturazioneSettings)
        {
            _fatturazioneService = fatturazioneService;
            _fatturazioneSettings = fatturazioneSettings;
        }

        public Task Run(TaskExecutionContext ctx, CancellationToken cancelToken = default)
        {
            var client = new ArubaClient(_fatturazioneSettings.BaseUrl, _fatturazioneSettings.BaseAuthUrl, _fatturazioneSettings.ArubaUsername, _fatturazioneSettings.ArubaPassword);

            var folder = new DirectoryInfo(Path.Combine(_fatturazioneSettings.AppDataFolder, _fatturazioneSettings.DeliveredFolderName));
            if (!folder.Exists)
                folder.Create();

            var xmlFiles = folder.GetFiles("*.xml");

            foreach (var file in xmlFiles)
            {
                var segments = file.Name.Split('.');

                if (int.TryParse(segments[1], out int invoiceId))
                {
                    var invoiceHistory = _fatturazioneService.GetLastInvoiceHistoryWithFileNameByInvoiceId(invoiceId);
                    if (invoiceHistory != null)
                    {
                        if (!string.IsNullOrEmpty(invoiceHistory.SdiFileName))
                        {
                            var res = client.DownloadUnsignedInvoice(new DownloadUnsignedInvoiceRequest
                            {
                                FileName = invoiceHistory.SdiFileName
                            });

                            if (res.ErrorCode == "WEX")
                                break;

                            if (res.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                if (res.ErrorCode == "0000")
                                {
                                    var fatturaResponse = res.Fatture?.FirstOrDefault();
                                    if (fatturaResponse != null)
                                    {
                                        try
                                        {
                                            var newHistory = CreateNewHistory(fatturaResponse, invoiceHistory);
                                            switch (newHistory.Status)
                                            {
                                                case InvoiceStatus.Accepted:
                                                case InvoiceStatus.Delivered:
                                                case InvoiceStatus.NotDeliveredToCustomer:
                                                    MoveFile(file, Path.Combine(_fatturazioneSettings.AppDataFolder, _fatturazioneSettings.DoneFolderName, DateTime.Now.Year.ToString(), invoiceId + ".xml"));
                                                    _fatturazioneService.InsertInvoiceHistory(newHistory);
                                                    File.WriteAllBytes(
                                                        Path.Combine(_fatturazioneSettings.AppDataFolder, _fatturazioneSettings.DoneFolderName, DateTime.Now.Year.ToString(), "W" + invoiceHistory.Invoice.OrderId + ".pdf"),
                                                        Convert.FromBase64String(res.PdfFile));
                                                    Logger.LogInformation("Saved PDF to file (invoice Id {InvoiceId})", invoiceHistory.InvoiceId);
                                                    break;
                                                case InvoiceStatus.TakingCharge:
                                                case InvoiceStatus.SentToCustomer:
                                                    Logger.LogWarning("Invoice Id {InvoiceId} status {Status}", invoiceHistory.InvoiceId, invoiceHistory.Status);
                                                    break;
                                                default:
                                                    MoveFile(file, Path.Combine(_fatturazioneSettings.AppDataFolder, _fatturazioneSettings.FailureFolderName, DateTime.Now.Year.ToString(), invoiceId + ".xml"));
                                                    _fatturazioneService.InsertInvoiceHistory(newHistory);
                                                    Logger.LogError("Unable to save PDF to file (invoice Id {InvoiceId})", invoiceHistory.InvoiceId);
                                                    break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.LogError(ex, "Error processing invoice Id {InvoiceId}", invoiceId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.LogWarning("No invoice record found for Id {InvoiceId}", invoiceId);
                    }
                }
            }

            return Task.CompletedTask;
        }

        private InvoiceHistory CreateNewHistory(Fattura fatturaResponse, InvoiceHistory currentStatus)
        {
            var stato = fatturaResponse.Stato?.Trim() ?? string.Empty;
            var newStatus = new InvoiceHistory
            {
                InvoiceId = currentStatus.InvoiceId,
                SdiFileName = currentStatus.SdiFileName,
                CreatedOnUtc = DateTime.UtcNow
            };

            if (stato.Equals("presa in carico", StringComparison.InvariantCultureIgnoreCase))
                newStatus.Status = InvoiceStatus.TakingCharge;
            else if (stato.Equals("errore elaborazione", StringComparison.InvariantCultureIgnoreCase))
            { newStatus.Status = InvoiceStatus.InvalidData; newStatus.ErrorDescription = stato; }
            else if (stato.Equals("inviata", StringComparison.InvariantCultureIgnoreCase))
                newStatus.Status = InvoiceStatus.SentToCustomer;
            else if (stato.Equals("scartata", StringComparison.InvariantCultureIgnoreCase))
            { newStatus.Status = InvoiceStatus.Rejected; newStatus.ErrorDescription = stato; }
            else if (stato.Equals("non consegnata", StringComparison.InvariantCultureIgnoreCase))
            { newStatus.Status = InvoiceStatus.NotDeliveredToCustomer; newStatus.ErrorDescription = stato; }
            else if (stato.Equals("recapito impossibile", StringComparison.InvariantCultureIgnoreCase))
            { newStatus.Status = InvoiceStatus.UnableToDeliver; newStatus.ErrorDescription = stato; }
            else if (stato.Equals("consegnata", StringComparison.InvariantCultureIgnoreCase))
                newStatus.Status = InvoiceStatus.Delivered;
            else if (stato.Equals("accettata", StringComparison.InvariantCultureIgnoreCase))
                newStatus.Status = InvoiceStatus.Accepted;
            else if (stato.Equals("rifiutata", StringComparison.InvariantCultureIgnoreCase))
            { newStatus.Status = InvoiceStatus.Refused; newStatus.ErrorDescription = stato; }
            else if (stato.Equals("decorrenza termini", StringComparison.InvariantCultureIgnoreCase))
            { newStatus.Status = InvoiceStatus.Expired; newStatus.ErrorDescription = stato; }
            else
            {
                newStatus.Status = InvoiceStatus.UnknownError;
                newStatus.ErrorDescription = fatturaResponse.Stato;
                Logger.LogError("An unknown status from Aruba SdI servers (file \"{SdiFile}\", invoice Id {InvoiceId}) was found: {Stato}",
                    currentStatus.SdiFileName, currentStatus.InvoiceId, fatturaResponse.Stato);
            }

            return newStatus;
        }

        private static void MoveFile(FileInfo sourceFile, string destinationPath)
        {
            var destinationFile = new FileInfo(destinationPath);
            if (!destinationFile.Directory.Exists)
                destinationFile.Directory.Create();

            if (destinationFile.Exists)
                File.Move(destinationFile.FullName, destinationFile.FullName + "." + DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            sourceFile.MoveTo(destinationPath);
        }
    }
}
