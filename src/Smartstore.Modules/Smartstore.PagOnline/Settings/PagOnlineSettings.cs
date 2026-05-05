using Smartstore.Core.Configuration;

namespace Smartstore.PagOnline.Settings
{
    public class PagOnlineSettings : ISettings
    {
        public string Tid { get; set; }

        public string Ksig { get; set; }

        public bool UseSandbox { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public string WebServiceUrl
        {
            get
            {
                if (UseSandbox)
                    return WebserviceUrlTest;

                return WebserviceUrlProd;
            }
        }

        public string WebserviceUrlProd => "https://pagamenti.unicredit.it/UNI_CG_SERVICES/services";

        public string WebserviceUrlTest => "https://testeps.netswgroup.it/UNI_CG_SERVICES/services";
    }
}
