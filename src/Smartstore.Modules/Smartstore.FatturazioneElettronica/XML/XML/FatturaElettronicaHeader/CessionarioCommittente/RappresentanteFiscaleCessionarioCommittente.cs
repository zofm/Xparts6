using System.Xml;
using Smartstore.FatturazioneElettronica.XML.Common;

namespace Smartstore.FatturazioneElettronica.XML.FatturaElettronicaHeader.CessionarioCommittente
{
    /// <summary>
    /// Represents a CessionarioCommittente.RappresentanteFiscale object.
    /// </summary>
    public class RappresentanteFiscaleCessionarioCommittente : DenominazioneNomeCognome
    {
        public RappresentanteFiscaleCessionarioCommittente()
        {

            IdFiscaleIVA = new IdFiscaleIVA();
        }
        public RappresentanteFiscaleCessionarioCommittente(XmlReader r) : base(r) { }

        /// <summary>
        /// Numero di identificazione fiscale ai fini IVA; i primi due caratteri rappresentano il paese ed i restanti il codice
        /// vero e proprio che, per i residenti in Italia, corrisponde al numero di partita IVA.
        /// </summary>
        [DataProperty]
        public IdFiscaleIVA IdFiscaleIVA { get; set; }
    }
}
