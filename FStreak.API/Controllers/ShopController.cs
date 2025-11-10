using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using System.Security.Claims;

namespace FStreak.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShopController : ControllerBase
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
        }

        /// <summary>
        /// Get all shop items
        /// </summary>
        [HttpGet("items")]
        [ProducesResponseType(typeof(List<ShopItemDto>), 200)]
        public async Task<IActionResult> GetShopItems([FromQuery] bool? activeOnly = true)
        {
            var result = await _shopService.GetShopItemsAsync(activeOnly);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Get shop item by ID
        /// </summary>
        [HttpGet("items/{id}")]
        [ProducesResponseType(typeof(ShopItemDto), 200)]
        public async Task<IActionResult> GetShopItem(Guid id)
        {
            var result = await _shopService.GetShopItemByIdAsync(id);
            if (!result.Succeeded)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        [HttpPost("orders")]
        [ProducesResponseType(typeof(ShopOrderDto), 201)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found");
            }

            var result = await _shopService.CreateOrderAsync(userId, dto);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return CreatedAtAction(nameof(GetOrder), new { id = result.Data!.Id }, result.Data);
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        [HttpGet("orders/{id}")]
        [ProducesResponseType(typeof(ShopOrderDto), 200)]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found");
            }

            var result = await _shopService.GetOrderByIdAsync(id, userId);
            if (!result.Succeeded)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Get user orders
        /// </summary>
        [HttpGet("orders")]
        [ProducesResponseType(typeof(List<ShopOrderDto>), 200)]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found");
            }

            var result = await _shopService.GetUserOrdersAsync(userId);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Buy item directly (quick purchase)
        /// </summary>
        [HttpPost("items/{id}/buy")]
        [ProducesResponseType(typeof(ShopOrderDto), 201)]
        public async Task<IActionResult> BuyItem(Guid id, [FromBody] BuyItemRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found");
            }

            request.ShopItemId = id;
            var result = await _shopService.BuyItemAsync(userId, request);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return CreatedAtAction(nameof(GetOrder), new { id = result.Data!.Id }, result.Data);
        }
    }
}

