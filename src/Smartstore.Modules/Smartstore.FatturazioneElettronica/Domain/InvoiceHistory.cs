using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Smartstore.Domain;

namespace Smartstore.FatturazioneElettronica.Domain
{
    [Table("FE_InvoiceHistory")]
    [Index(nameof(InvoiceId))]
    public class InvoiceHistory : BaseEntity
    {
        public int InvoiceId { get; set; }

        public InvoiceStatus Status { get; set; }

        [StringLength(500)]
        public string SdiFileName { get; set; }

        [StringLength(100)]
        public string ErrorCode { get; set; }

        [StringLength(2000)]
        public string ErrorDescription { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public virtual Invoice Invoice { get; set; }
    }

    public enum InvoiceStatus
    {
        SentToSdi = 0,
        ErrorSendingToSdi = 1,
        TakingCharge = 2,
        InvalidData = 3,
        SentToCustomer = 4,
        Rejected = 5,
        NotDeliveredToCustomer = 6,
        UnableToDeliver = 7,
        Delivered = 8,
        Accepted = 9,
        Refused = 10,
        Expired = 11,
        UnknownError = 12
    }
}
