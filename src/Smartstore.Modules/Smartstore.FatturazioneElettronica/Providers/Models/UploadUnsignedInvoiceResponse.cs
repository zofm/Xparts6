using System.Net;

namespace Smartstore.FatturazioneElettronica.Providers.Models
{
    public class UploadUnsignedInvoiceResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string FileName { get; set; }
    }
}
