using System.Xml;

namespace Smartstore.FatturazioneElettronica.XML.FatturaElettronicaHeader.CedentePrestatore
{
    public class SedeCedentePrestatore : Common.Località
    {
        public SedeCedentePrestatore() { } 
        public SedeCedentePrestatore(XmlReader r) : base(r) { } 
    }
}
