using Smartstore.FatturazioneElettronica.Providers.Models;

namespace Smartstore.FatturazioneElettronica.Providers
{
    public interface IFatturazioneElettronicaClient
    {
        UploadUnsignedInvoiceResponse UploadUnsignedInvoice(UploadUnsignedInvoiceRequest req);

        DownloadUnsignedInvoiceResponse DownloadUnsignedInvoice(DownloadUnsignedInvoiceRequest req);
    }
}
