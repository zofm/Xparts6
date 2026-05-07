using System.ComponentModel;

namespace Smartstore.FatturazioneElettronica
{
    public enum CondizioniPagamento
    {
        [Description("TP01")]
        Rate,

        [Description("TP02")]
        Completo,

        [Description("TP03")]
        Anticipo
    }

    public enum ModalitaPagamento
    {
        [Description("MP01")]
        Contanti,

        [Description("MP02")]
        Assegno,

        [Description("MP03")]
        AssegnoCircolare,

        [Description("MP04")]
        ContantiPressoTesoreria,

        [Description("MP05")]
        Bonifico,

        [Description("MP06")]
        VagliaCambiario,

        [Description("MP07")]
        BollettinoBancario,

        [Description("MP08")]
        CartaDiPagamento,

        [Description("MP09")]
        RID,

        [Description("MP10")]
        RIDUtenze,

        [Description("MP11")]
        RIDVeloce,

        [Description("MP12")]
        Riba,

        [Description("MP13")]
        MAV,

        [Description("MP14")]
        QuietanzaErarioStato,

        [Description("MP15")]
        GirocontoSuContiDiContabilitaSpeciale,

        [Description("MP16")]
        DomiciliazioneBancaria,

        [Description("MP17")]
        DomiciliazionePostale,

        [Description("MP18")]
        BollettinoDiCCPostale,

        [Description("MP19")]
        SEPADirectDebit,

        [Description("MP20")]
        SEPADirectDebitCORE,

        [Description("MP21")]
        SEPADirectDebitB2B,

        [Description("MP22")]
        TrattenutaSuSommeGiaRiscosse
    }

    public enum RegimeFiscale
    {
        [Description("RF01")]
        Ordinario,

        [Description("RF02")]
        ContribuentiMinimi,

        [Description("RF16")]
        IvaPerCassaPA,

        [Description("RF17")]
        IvaPerCassa,

        [Description("RF18")]
        Altro,

        [Description("RF19")]
        Forfettario
    }

    public enum EsenzioneIva
    {
        [Description("N1")]
        EsclusaExArt15 = 0,

        [Description("N2.1")]
        NonSoggette = 1,

        [Description("N2.2")]
        NonSoggetteAltriCasi = 7,

        [Description("N3.1")]
        NonImponibiliEsportazioni = 8,

        [Description("N3.2")]
        NonImponibili = 2,

        [Description("N3.3")]
        NonImponibiliVersoRsm = 9,

        [Description("N3.4")]
        NonImponibiliCessioniEsportazione = 10,

        [Description("N3.5")]
        NonImponibiliDichiarazioniIntento = 11,

        [Description("N3.6")]
        NonImponibiliAltre = 12,

        [Description("N4")]
        Esenti = 3,

        [Description("N5")]
        RegimeDelMargineOIvaNonEspostaInFattura = 4,

        [Description("N6")]
        InversioneContabile = 5,

        [Description("N7")]
        IvaAssoltaInAltroStatoUE = 6
    }

    public enum TipoSoggetto
    {
        Privato,
        Azienda,
        PubblicaAmministrazione
    }
}
