using System.ComponentModel.DataAnnotations;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public enum ShopItemType
    {
        Mascot,
        Token,
        Consumable,
        Voucher
    }

    public class ShopItem : BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public ShopItemType Type { get; set; }

        public decimal Price { get; set; }

        public int PricePoints { get; set; }

        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(2000)]
        public string? Image { get; set; }

        // Navigation properties
        public virtual ICollection<ShopOrderItem> OrderItems { get; set; } = new HashSet<ShopOrderItem>();
    }
}

