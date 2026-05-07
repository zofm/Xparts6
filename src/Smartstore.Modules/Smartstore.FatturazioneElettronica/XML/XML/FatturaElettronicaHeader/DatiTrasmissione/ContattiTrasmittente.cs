using System.Xml;
using Smartstore.FatturazioneElettronica.XML.Common;

namespace Smartstore.FatturazioneElettronica.XML.FatturaElettronicaHeader.DatiTrasmissione
{
    /// <summary>
    /// Represents a ContattiTrasmittente object
    /// </summary>
    public class ContattiTrasmittente : BaseClassSerializable
    {
        public ContattiTrasmittente() { }
        public ContattiTrasmittente(XmlReader r) : base(r) { }

        /// <summary>
        /// Contatto telefonico fisso o mobile.
        /// </summary>
        [DataProperty]
        public string Telefono { get; set; }

        /// <summary>
        /// Indirizzo di posta elettronica.
        /// </summary>
        [DataProperty]
        public string Email { get; set; }
    }
}
