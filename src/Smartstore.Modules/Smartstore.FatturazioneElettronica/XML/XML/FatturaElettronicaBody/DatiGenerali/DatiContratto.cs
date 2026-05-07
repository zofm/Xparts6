using System.Xml;

namespace Smartstore.FatturazioneElettronica.XML.FatturaElettronicaBody.DatiGenerali
{
    /// <summary>
    /// Informazioni relative al contratto.
    /// </summary>
    public class DatiContratto : Common.DatiDocumento
    {
        public DatiContratto() { }
        public DatiContratto(XmlReader r) : base(r) { }
    }
}
