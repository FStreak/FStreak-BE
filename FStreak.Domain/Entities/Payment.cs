using FStreak.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Domain.Entities
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public long OrderCode { get; set; }

        public decimal Amount { get; set; }

        public string PlanId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // e.g., Pending, Paid, Faile, Cancelled

        public string Description { get; set; } = string.Empty;
        public string? PaymentUrl { get; set; }
        public string? TransactionReference { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CompleteAt { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
