using FStreak.Application.DTOs;

namespace FStreak.Application.Services.Interface
{
    public interface IShopService
    {
        /// <summary>
        /// Get all shop items
        /// </summary>
        Task<Result<List<ShopItemDto>>> GetShopItemsAsync(bool? activeOnly = true);

        /// <summary>
        /// Get shop item by ID
        /// </summary>
        Task<Result<ShopItemDto>> GetShopItemByIdAsync(Guid id);

        /// <summary>
        /// Create a new order
        /// </summary>
        Task<Result<ShopOrderDto>> CreateOrderAsync(string userId, CreateOrderDto dto);

        /// <summary>
        /// Get order by ID
        /// </summary>
        Task<Result<ShopOrderDto>> GetOrderByIdAsync(Guid id, string userId);

        /// <summary>
        /// Get user orders
        /// </summary>
        Task<Result<List<ShopOrderDto>>> GetUserOrdersAsync(string userId);

        /// <summary>
        /// Buy item directly (quick purchase)
        /// </summary>
        Task<Result<ShopOrderDto>> BuyItemAsync(string userId, BuyItemRequest request);

        /// <summary>
        /// Create shop item (Admin only)
        /// </summary>
        Task<Result<ShopItemDto>> CreateShopItemAsync(CreateShopItemDto dto);

        /// <summary>
        /// Update shop item (Admin only)
        /// </summary>
        Task<Result<ShopItemDto>> UpdateShopItemAsync(Guid id, UpdateShopItemDto dto);

        /// <summary>
        /// Delete shop item (Admin only)
        /// </summary>
        Task<Result<bool>> DeleteShopItemAsync(Guid id);
    }
}

