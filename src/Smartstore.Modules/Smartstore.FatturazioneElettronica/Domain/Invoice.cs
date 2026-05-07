using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Smartstore.Domain;
using Smartstore.FatturazioneElettronica.Domain;

namespace Smartstore.FatturazioneElettronica.Domain
{
    [Table("FE_Invoice")]
    [Index(nameof(OrderId))]
    public class Invoice : BaseEntity
    {
        public int OrderId { get; set; }

        public int Year { get; set; }

        public int Number { get; set; }

        public int? ExemptionId { get; set; }

        public DateTime? CreatedOnUtc { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public bool HasXmlFile { get; set; }

        [StringLength(2000)]
        public string Causal { get; set; }

        private ICollection<InvoiceHistory> _history;
        public virtual ICollection<InvoiceHistory> History
        {
            get => _history ??= new HashSet<InvoiceHistory>();
            protected set => _history = value;
        }
    }
}
