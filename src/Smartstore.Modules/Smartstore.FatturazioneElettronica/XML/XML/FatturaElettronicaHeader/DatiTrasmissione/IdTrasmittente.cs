using System.Xml;
using Smartstore.FatturazioneElettronica.XML.Common;

namespace Smartstore.FatturazioneElettronica.XML.FatturaElettronicaHeader.DatiTrasmissione
{
    public class IdTrasmittente : IdFiscaleIVA
    {
        public IdTrasmittente() { }
        public IdTrasmittente(XmlReader r) : base(r) { }
    }
}
