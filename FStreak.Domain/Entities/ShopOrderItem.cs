using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class ShopOrderItem : BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid ShopItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAtPurchase { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual ShopOrder Order { get; set; } = null!;

        [ForeignKey("ShopItemId")]
        public virtual ShopItem ShopItem { get; set; } = null!;
    }
}

