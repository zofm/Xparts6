using Smartstore.ComponentModel;

namespace Smartstore.FatturazioneElettronica.Models
{
    [CustomModelPart]
    public class CustomerSdiCodeModel : ModelBase
    {
        public int CustomerId { get; set; }

        [LocalizedDisplay("Plugins.SmartStore.FE.Customer.SdiCode")]
        public string SdiCode { get; set; }
    }
}
