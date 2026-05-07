using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Smartstore.FatturazioneElettronica.Providers.Aruba.Models
{
    public class BasePOSTResponse<RES>
    {
        public RES Response { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }

    public class BaseGETResponse<RES>
    {
        public RES Response { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}
