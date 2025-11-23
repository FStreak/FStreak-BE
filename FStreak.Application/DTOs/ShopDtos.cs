namespace FStreak.Application.DTOs
{
    public class ShopItemDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int PricePoints { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public string? MetadataJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ShopOrderDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TotalPoints { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<ShopOrderItemDto> OrderItems { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ShopOrderItemDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ShopItemId { get; set; }
        public ShopItemDto? ShopItem { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }

    public class CreateShopItemDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int PricePoints { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;
        public string? MetadataJson { get; set; }
    }

    public class UpdateShopItemDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? PricePoints { get; set; }
        public int? Stock { get; set; }
        public bool? IsActive { get; set; }
        public string? MetadataJson { get; set; }
    }

    public class CreateOrderDto
    {
        public List<OrderItemRequest> Items { get; set; } = new();
        public string PayWith { get; set; } = "Points"; // Points or Money
    }

    public class OrderItemRequest
    {
        public Guid ShopItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class BuyItemRequest
    {
        public Guid ShopItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public string PayWith { get; set; } = "Points";
    }
}

