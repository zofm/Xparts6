using System.Net;

namespace Smartstore.FatturazioneElettronica.Providers.Models
{
    public class DownloadUnsignedInvoiceResponse
    {
        public string Id { get; set; }
        public Azienda AziendaInviante { get; set; }
        public Azienda AziendaRicevente { get; set; }
        public string TipoFattura { get; set; }
        public string TipoDocumento { get; set; }
        public string File { get; set; }
        public string PdfFile { get; set; }
        public string NomeFile { get; set; }
        public List<Fattura> Fatture { get; set; }
        public string Username { get; set; }
        public string DataUltimoAggiornamento { get; set; }
        public string IdSDI { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }

    public class Azienda
    {
        public string Descrizione { get; set; }
        public string CodicePaese { get; set; }
        public string PartitaIva { get; set; }
        public string CodiceFiscale { get; set; }
    }

    public class Fattura
    {
        public string Data { get; set; }
        public string Numero { get; set; }
        public string Stato { get; set; }
    }
}
