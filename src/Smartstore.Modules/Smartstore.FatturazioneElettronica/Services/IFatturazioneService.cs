using Smartstore.FatturazioneElettronica.Domain;

namespace Smartstore.FatturazioneElettronica.Services
{
    public partial interface IFatturazioneService
    {
        FileInfo CreateInvoiceXml(int orderId);

        Invoice GetInvoiceByOrderId(int orderId);

        int GetLastInvoiceNumber(int year);

        bool CanInvoiceBeDeleted(int orderId);

        bool CheckInvoiceForCustomerId(int customerId, int orderId);

        void CreateInvoice(int orderId, int? exemptionId, string causal);

        void RecreateInvoice(int orderId, int invoiceId, int? exemptionId, string causal);

        void UpdateInvoice(Invoice record);

        void DeleteInvoice(Invoice record);

        void DeleteInvoiceByOrderId(int orderId);

        void InsertInvoiceHistory(InvoiceHistory item);

        IEnumerable<InvoiceHistory> GetInvoiceHistoriesByInvoiceId(int invoiceId);

        InvoiceHistory GetLastInvoiceHistoryWithFileNameByInvoiceId(int invoiceId);

        IEnumerable<Invoice> GetAllInvoicesToCreateXml();

        void NormalizeAddresses();
    }
}
