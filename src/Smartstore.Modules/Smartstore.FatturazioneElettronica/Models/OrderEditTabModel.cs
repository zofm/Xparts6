using Smartstore.FatturazioneElettronica.Domain;
using Smartstore.FatturazioneElettronica.Services;

namespace Smartstore.FatturazioneElettronica.Models
{
    [CustomModelPart]
    public class OrderEditTabModel : ModelBase
    {
        public int EntityId { get; set; }

        public int? InvoiceNumber { get; set; }

        public int? InvoiceYear { get; set; }

        public int? InvoiceId { get; set; }

        public int? ExemptionId { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.FE.Labels.Causal")]
        public string Causal { get; set; }

        public bool CanBeDeleted { get; set; }

        public bool CanBeRecreated { get; set; }

        public DateTime? CreateDateUtc { get; set; }

        public List<InvoiceHistoryModel> History { get; set; }

        public int SelectedExemptId { get; set; }

        public string ErrorMessage { get; set; }

        public void Bind(IFatturazioneService service)
        {
            var invoice = service.GetInvoiceByOrderId(EntityId);
            if (invoice != null)
            {
                InvoiceId = invoice.Id;
                InvoiceNumber = invoice.Number;
                InvoiceYear = invoice.Year;
                CreateDateUtc = invoice.CreatedOnUtc;
                ExemptionId = invoice.ExemptionId;
                Causal = invoice.Causal;
                CanBeDeleted = service.CanInvoiceBeDeleted(EntityId);
                History = service.GetInvoiceHistoriesByInvoiceId(invoice.Id).Select(x => new InvoiceHistoryModel
                {
                    Status = x.Status,
                    ErrorCode = x.ErrorCode,
                    ErrorDescription = x.ErrorDescription,
                    CreatedOnUtc = x.CreatedOnUtc,
                    SdiFileName = x.SdiFileName
                }).ToList();

                var lastStatus = History.LastOrDefault();
                if (lastStatus != null)
                {
                    if (lastStatus.Status != InvoiceStatus.SentToSdi
                        && lastStatus.Status != InvoiceStatus.Accepted
                        && lastStatus.Status != InvoiceStatus.Delivered
                        && lastStatus.Status != InvoiceStatus.NotDeliveredToCustomer)
                    {
                        CanBeRecreated = true;
                    }
                }
            }
        }
    }
}
