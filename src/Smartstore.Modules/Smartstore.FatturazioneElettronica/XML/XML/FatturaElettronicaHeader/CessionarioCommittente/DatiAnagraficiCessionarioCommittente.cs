using System.Xml;
using Smartstore.FatturazioneElettronica.XML.Common;

namespace Smartstore.FatturazioneElettronica.XML.FatturaElettronicaHeader.CessionarioCommittente
{
    public class DatiAnagraficiCessionarioCommittente : DatiAnagrafici
    {
        /// <summary>
        /// Dati anagrafici, professionali e fiscali del cessionario / committente.
        /// </summary>
        public DatiAnagraficiCessionarioCommittente() { }
        public DatiAnagraficiCessionarioCommittente(XmlReader r) : base(r) { }
    }
}
