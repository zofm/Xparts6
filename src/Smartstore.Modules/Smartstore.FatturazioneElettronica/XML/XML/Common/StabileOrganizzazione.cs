using System.Xml;

namespace Smartstore.FatturazioneElettronica.XML.Common
{
    public class StabileOrganizzazione : Località
    {
        public StabileOrganizzazione() { } 
        public StabileOrganizzazione(XmlReader r) : base(r) { } 
    }
}
