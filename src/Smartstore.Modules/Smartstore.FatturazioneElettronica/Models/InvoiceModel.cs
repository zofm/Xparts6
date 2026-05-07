using System.ComponentModel;

namespace Smartstore.FatturazioneElettronica.Models
{
    public enum TipoDocumento
    {
        [Description("TD01")]
        Fattura,

        [Description("TD02")]
        AccontoAnticipo,

        [Description("TD04")]
        NotaDiCredito,

        [Description("TD20")]
        Autofattura
    }

    public enum Divisa
    {
        [Description("EUR")]
        EUR
    }

    public enum EsigibilitaIva
    {
        [Description("I")]
        Immediata,

        [Description("D")]
        Differita,

        [Description("S")]
        ScissionePagamenti
    }

    public enum TipoSconto
    {
        Percentuale,
        Importo
    }

    public enum DestinatarioFattura
    {
        Privato,
        Azienda,
        PubblicaAmministrazione
    }

    public class FatturaModel
    {
        public DestinatarioFattura DestinatarioFattura { get; } = DestinatarioFattura.Privato;

        public FatturaModel(DestinatarioFattura destinatario)
        {
            DestinatarioFattura = destinatario;
            Dettagli = new List<Dettaglio>();
        }

        public string ProgressivoInvio { get; set; }
        public string IdCodiceTrasmissione { get; set; }
        public string IdPaeseTrasmissione { get; set; }
        public string CodiceDestinatario { get; set; }
        public string PECDestinatario { get; set; }
        public Cedente Cedente { get; set; }
        public Cessionario Cessionario { get; set; }
        public Intestazione Intestazione { get; set; }
        public List<Dettaglio> Dettagli { get; set; }
        public Pagamento Pagamento { get; set; }
    }

    public class Pagamento
    {
        public Pagamento()
        {
            Dettagli = new List<DettaglioPagamento>();
        }

        public CondizioniPagamento CondizioniPagamento { get; set; }
        public List<DettaglioPagamento> Dettagli { get; set; }
    }

    public class DettaglioPagamento
    {
        public ModalitaPagamento ModalitaPagamento { get; set; }
        public decimal Importo { get; set; }
        public decimal Imposta { get; set; }
        public DateTime DataScadenza { get; set; }
    }

    public class Dettaglio
    {
        public string Descrizione { get; set; }
        public int Quantita { get; set; }
        public decimal PrezzoUnitario { get; set; }
        public decimal AliquotaIva { get; set; }
        public TipoPrestazione TipoPrestazione { get; set; }
        public EsenzioneIva? EsenzioneIva { get; set; }
        public List<Sconto> Sconti { get; set; }

        public decimal PrezzoTotale
            => Math.Round(PrezzoUnitarioScontato * Quantita, 2);

        public decimal ImpostaTotale
        {
            get
            {
                if (AliquotaIva > 0)
                {
                    decimal iva = AliquotaIva * 0.01M;
                    return Math.Round(PrezzoTotale * iva, 2);
                }
                return 0;
            }
        }

        public decimal PrezzoUnitarioScontato
        {
            get
            {
                if (Sconti != null)
                {
                    var scontoTotale = 0M;
                    Sconti.ForEach(x =>
                    {
                        if (x.TipoSconto == TipoSconto.Percentuale)
                            scontoTotale += Math.Round((PrezzoUnitario * x.Valore) / 100, 2);
                        else
                            scontoTotale += x.Valore;
                    });
                    return PrezzoUnitario - scontoTotale;
                }
                return PrezzoUnitario;
            }
        }
    }

    public class Sconto
    {
        public TipoSconto TipoSconto { get; set; }
        public decimal Valore { get; set; }
    }

    public enum TipoPrestazione
    {
        Standard,
        Sconto
    }

    public class Intestazione
    {
        public TipoDocumento TipoDocumento { get; set; }
        public DateTime DataEmissione { get; set; }
        public Divisa Divisa { get; set; }
        public string NumeroDocumento { get; set; }
        public string Causale { get; set; }
        public EsigibilitaIva EsigibilitaIva { get; set; }
    }

    public class Soggetto
    {
        public TipoSoggetto TipoSoggetto { get; set; }
        public string Denominazione { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string CodiceFiscale { get; set; }
        public string PartitaIva { get; set; }
        public string Indirizzo { get; set; }
        public string NumeroCivico { get; set; }
        public string Cap { get; set; }
        public string Comune { get; set; }
        public string Provincia { get; set; }
        public string Paese { get; set; }
        public string CodiceEORI { get; set; }
        public IdFiscaleIva IdFiscaleIva { get; set; }
    }

    public class Cedente : Soggetto
    {
        public RegimeFiscale RegimeFiscale { get; set; }
    }

    public class Cessionario : Soggetto { }

    public class IdFiscaleIva
    {
        public string CodicePaese { get; set; }
        public string PartitaIva { get; set; }
    }
}
