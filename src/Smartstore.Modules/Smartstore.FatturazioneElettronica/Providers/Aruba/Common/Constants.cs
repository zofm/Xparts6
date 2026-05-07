using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartstore.FatturazioneElettronica.Providers.Aruba.Common
{
    public static class Constants
    {
        /*public static string BaseAuthUrl
        {
            get
            {
                //return "https://demoauth.fatturazioneelettronica.aruba.it";
                return "https://auth.fatturazioneelettronica.aruba.it";
            }
        }

        public static string BaseUrl
        {
            get
            {
                //return "https://demows.fatturazioneelettronica.aruba.it";
                return "https://ws.fatturazioneelettronica.aruba.it";
            }
        }*/


        public static string SigninPath
        {
            get
            {
                return "/auth/signin";
            }
        }

        public static string UploadInvoicePath
        {
            get
            {
                return "/services/invoice/upload";
            }
        }

        public static string DownloadInvoicePath
        {
            get
            {
                return "/services/invoice/out/getByFilename";
            }
        }

        /*public static string Username
        {
            get
            {
                //return "PREMIUM00000000289";
                return "asfdasfads";
            }
        }

        public static string Password
        {
            get
            {
                //return "bweOea[sx/45m";
                return "asfdadfsdf";
            }
        }*/
    }
}
