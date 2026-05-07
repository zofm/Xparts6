using System.Xml;

namespace Smartstore.FatturazioneElettronica.XML.FatturaElettronicaHeader.CessionarioCommittente
{
    public class SedeCessionarioCommittente : Common.Località
    {
        public SedeCessionarioCommittente() { } 

        public SedeCessionarioCommittente(XmlReader r) : base(r) { } 
    }
}
