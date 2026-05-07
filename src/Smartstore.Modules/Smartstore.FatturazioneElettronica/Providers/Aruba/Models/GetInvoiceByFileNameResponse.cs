using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartstore.FatturazioneElettronica.Providers.Aruba.Models
{
    public class GetInvoiceByFileNameResponse
    {
        public string ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        public string Id { get; set; }

        public Company Sender { get; set; }

        public Company Receiver { get; set; }

        public string InvoiceType { get; set; }

        public string DocType { get; set; }

        public string File { get; set; }

        public string PdfFile { get; set; }

        public string FileName { get; set; }

        public List<ArubaInvoice> Invoices { get; set; }

        public string Username { get; set; }

        public string LastUpdate { get; set; }

        public string IdSDI { get; set; }
    }

    public class Company
    {
        public string Description { get; set; }

        public string CountryCode { get; set; }

        public string VatCode { get; set; }

        public string FiscalCode { get; set; }
    }

    public class ArubaInvoice
    {
        public string InvoiceDate { get; set; }

        public string Number { get; set; }

        public string Status { get; set; }
    }
}
