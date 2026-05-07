using Smartstore.FatturazioneElettronica.Domain;

namespace Smartstore.FatturazioneElettronica.Models
{
    public class InvoiceHistoryModel
    {
        public InvoiceStatus Status { get; set; }

        public string SdiFileName { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
