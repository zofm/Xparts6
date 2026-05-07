using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartstore.FatturazioneElettronica.Providers.Aruba.Models
{
    public class UploadInvoiceRequest
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "dataFile")]
        public string DataFile { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "credentials")]
        public string Credentials { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "domain")]
        public string Domain { get; set; }
    }
}
