using System.Xml;

namespace Smartstore.FatturazioneElettronica.XML.FatturaElettronicaBody.DatiGenerali
{
    public class IndirizzoResa : Common.Località
    {
        public IndirizzoResa() { } 
        public IndirizzoResa(XmlReader r) : base(r) { } 
    }
}
