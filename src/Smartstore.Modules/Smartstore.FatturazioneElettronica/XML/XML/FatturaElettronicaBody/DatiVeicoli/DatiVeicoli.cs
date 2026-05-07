using System;
using System.Xml;
using Smartstore.FatturazioneElettronica.XML.Common;

namespace Smartstore.FatturazioneElettronica.XML.FatturaElettronicaBody.DatiVeicoli
{
    /// <summary>
    /// Dati relativi ai veicoli di cui all'art. 38 comma 4 del ddl 331 del 1993.
    /// </summary>
    public class DatiVeicoli : BaseClassSerializable
    {
        public DatiVeicoli() { }
        public DatiVeicoli(XmlReader r) : base(r) { }

        /// <summary>
        /// Data di prima immatricolazione o iscrizione nei pubblici registri.
        /// </summary>
        [DataProperty]
        public DateTime? Data { get; set; }

        /// <summary>
        /// Totale chilometri percorsi, oppure totale ora navigate o volate.
        /// </summary>
        [DataProperty]
        public string TotalePercorso { get; set; }
    }
}
