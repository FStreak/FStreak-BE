using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public enum ShopOrderStatus
    {
        Pending,
        Completed,
        Cancelled
    }

    public class ShopOrder : BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public int TotalPoints { get; set; }

        [Required]
        public ShopOrderStatus Status { get; set; } = ShopOrderStatus.Pending;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ICollection<ShopOrderItem> OrderItems { get; set; } = new HashSet<ShopOrderItem>();
    }
}

