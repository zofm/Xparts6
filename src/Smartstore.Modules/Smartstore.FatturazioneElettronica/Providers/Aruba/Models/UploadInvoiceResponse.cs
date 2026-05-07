using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartstore.FatturazioneElettronica.Providers.Aruba.Models
{
    public class UploadInvoiceResponse
    {
        public string ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        public string UploadFileName { get; set; }
    }
}
