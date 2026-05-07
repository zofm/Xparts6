using System.Text.RegularExpressions;

namespace Smartstore.FatturazioneElettronica
{
    public class Utilities
    {
        public static string InsertSpacesBetweenCamelCaseWordsString(string text)
        {
            return Regex.Replace(text, "(\\B[A-Z])", " $1");
        }

        public static string Normalizza(string text)
        {
            return text.Replace("'", "'");
        }

        public static string TogliCaratteri(string text)
        {
            return text
                .Replace("-", "")
                .Replace(" ", "")
                .Replace(".", "")
                .Replace("_", "");
        }

        public static string RitornaDescrizioneEsenzione(EsenzioneIva esenzione)
        {
            return esenzione switch
            {
                EsenzioneIva.NonImponibiliEsportazioni => "NON IMPONIBILE ESPORTAZIONI ART. 8 DPR 633/1972",
                EsenzioneIva.NonImponibili => "ESENTE ART. 41 e 42 DL 331/1993",
                EsenzioneIva.EsclusaExArt15 => "ESCLUSA ART. 15 DPR 633/1972",
                EsenzioneIva.Esenti => "ESENTE ART. 42 DL 331/1993",
                EsenzioneIva.InversioneContabile => "INVERSIONE CONTABILE PROVV. AE 27 MARZO 2017 n. 58793",
                EsenzioneIva.IvaAssoltaInAltroStatoUE => "IVA ASSOLTA IN ALTRO STATO UE ART. 40 DL 331/1993",
                EsenzioneIva.NonSoggette => "NON SOGGETTA DPR 633/1972",
                EsenzioneIva.RegimeDelMargineOIvaNonEspostaInFattura => "REGIME DEL MARGINE ART. 36 DL 41/95",
                _ => ""
            };
        }
    }
}
