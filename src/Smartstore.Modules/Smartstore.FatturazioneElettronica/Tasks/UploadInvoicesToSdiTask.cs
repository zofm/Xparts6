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
    public class UploadInvoicesToSdiTask : ITask
    {
        private readonly IFatturazioneService _fatturazioneService;
        private readonly FatturazioneElettronicaSettings _fatturazioneSettings;

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public UploadInvoicesToSdiTask(
            IFatturazioneService fatturazioneService,
            FatturazioneElettronicaSettings fatturazioneSettings)
        {
            _fatturazioneService = fatturazioneService;
            _fatturazioneSettings = fatturazioneSettings;
        }

        public Task Run(TaskExecutionContext ctx, CancellationToken cancelToken = default)
        {
            var client = new ArubaClient(_fatturazioneSettings.BaseUrl, _fatturazioneSettings.BaseAuthUrl, _fatturazioneSettings.ArubaUsername, _fatturazioneSettings.ArubaPassword);

            var folder = new DirectoryInfo(Path.Combine(_fatturazioneSettings.AppDataFolder, _fatturazioneSettings.WaitingFolderName));
            if (!folder.Exists)
                folder.Create();

            var xmlFiles = folder.GetFiles("*.xml");

            foreach (var file in xmlFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FullName);
                if (fileNameWithoutExtension.IndexOf('.') > -1)
                    fileNameWithoutExtension = fileNameWithoutExtension.Split('.')[1];

                if (int.TryParse(fileNameWithoutExtension, out int invoiceId))
                {
                    var res = client.UploadUnsignedInvoice(new UploadUnsignedInvoiceRequest
                    {
                        XmlFilePath = file.FullName
                    });

                    switch (res.ErrorCode)
                    {
                        case "WEX":
                        case "FCF":
                        case "SER":
                        case "SEF":
                            Logger.LogError("Unable to send invoice {FileName}: {Description}", file.Name, res.ErrorDescription);
                            break;
                        default:
                            if (res.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                var historyItem = new InvoiceHistory
                                {
                                    InvoiceId = invoiceId,
                                    SdiFileName = res.FileName,
                                    Status = InvoiceStatus.SentToSdi,
                                    CreatedOnUtc = DateTime.UtcNow
                                };

                                if (res.ErrorCode == "0000")
                                {
                                    MoveFile(file, Path.Combine(_fatturazioneSettings.AppDataFolder, _fatturazioneSettings.DeliveredFolderName, file.Name));
                                }
                                else
                                {
                                    historyItem.Status = InvoiceStatus.ErrorSendingToSdi;
                                    historyItem.ErrorCode = res.ErrorCode;
                                    historyItem.ErrorDescription = res.ErrorDescription;
                                    MoveFile(file, Path.Combine(_fatturazioneSettings.AppDataFolder, _fatturazioneSettings.FailureFolderName, DateTime.Now.Year.ToString(), file.Name));
                                }

                                _fatturazioneService.InsertInvoiceHistory(historyItem);
                            }
                            else
                            {
                                Logger.LogError("Unable to send invoice {FileName} for a network related issue: {StatusCode}", file.Name, res.StatusCode);
                            }
                            break;
                    }
                }
                else
                {
                    Logger.LogWarning("XML file name is not valid: {FileName}", file.Name);
                }
            }

            return Task.CompletedTask;
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
