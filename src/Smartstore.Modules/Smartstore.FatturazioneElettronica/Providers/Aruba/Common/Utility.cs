using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Smartstore.FatturazioneElettronica.Providers.Aruba.Common
{
    public static class Utility
    {
        public static string GetBase64EncodedXML(string filePath)
        {
            Byte[] bytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(bytes);
        }
    }
}
