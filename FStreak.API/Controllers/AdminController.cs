using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using FStreak.Domain.Entities;
using System.Security.Claims;

namespace FStreak.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin,admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IAchievementService _achievementService;
        private readonly IShopService _shopService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IAchievementService achievementService,
            IShopService shopService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _achievementService = achievementService;
            _shopService = shopService;
        }

        #region User Management

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        [HttpGet("users")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var users = _userManager.Users
                    .Where(u => !u.IsDeleted)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var userDtos = users.Select(u => new
                {
                    UserId = u.Id,
                    Email = u.Email ?? string.Empty,
                    Username = u.UserName ?? string.Empty,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    AvatarUrl = u.AvatarUrl,
                    CurrentStreak = u.CurrentStreak,
                    LongestStreak = u.LongestStreak,
                    CreatedAt = u.CreatedAt
                }).ToList();

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to get users: {ex.Message}");
            }
        }

        /// <summary>
        /// Get user by ID (Admin only)
        /// </summary>
        [HttpGet("users/{id}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null || user.IsDeleted)
                {
                    return NotFound("User not found");
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userDto = new
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    Username = user.UserName ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AvatarUrl = user.AvatarUrl,
                    CurrentStreak = user.CurrentStreak,
                    LongestStreak = user.LongestStreak,
                    Roles = roles,
                    CreatedAt = user.CreatedAt
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to get user: {ex.Message}");
            }
        }

        /// <summary>
        /// Add role to user (Admin only)
        /// </summary>
        [HttpPost("users/{id}/roles")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> AddRoleToUser(string id, [FromBody] AddRoleRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var roleExists = await _roleManager.RoleExistsAsync(request.Role);
                if (!roleExists)
                {
                    return BadRequest($"Role '{request.Role}' does not exist");
                }

                var isInRole = await _userManager.IsInRoleAsync(user, request.Role);
                if (isInRole)
                {
                    return BadRequest($"User already has role '{request.Role}'");
                }

                var result = await _userManager.AddToRoleAsync(user, request.Role);
                if (!result.Succeeded)
                {
                    return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return Ok(new { message = $"Role '{request.Role}' added to user successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to add role: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove role from user (Admin only)
        /// </summary>
        [HttpDelete("users/{id}/roles/{role}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveRoleFromUser(string id, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var isInRole = await _userManager.IsInRoleAsync(user, role);
                if (!isInRole)
                {
                    return BadRequest($"User does not have role '{role}'");
                }

                var result = await _userManager.RemoveFromRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return Ok(new { message = $"Role '{role}' removed from user successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to remove role: {ex.Message}");
            }
        }

        /// <summary>
        /// Lock or unlock user account (Admin only)
        /// </summary>
        [HttpPut("users/{id}/lock")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> LockUser(string id, [FromBody] LockUserRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                if (request.IsLocked)
                {
                    user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // Lock indefinitely
                }
                else
                {
                    user.LockoutEnd = null; // Unlock
                }

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return Ok(new { message = $"User {(request.IsLocked ? "locked" : "unlocked")} successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to lock/unlock user: {ex.Message}");
            }
        }

        #endregion

        #region Achievement Management

        /// <summary>
        /// Create a new achievement (Admin only)
        /// </summary>
        [HttpPost("achievements")]
        [ProducesResponseType(typeof(AchievementDto), 201)]
        public async Task<IActionResult> CreateAchievement([FromBody] CreateAchievementDto dto)
        {
            var result = await _achievementService.CreateAchievementAsync(dto);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return CreatedAtAction(nameof(GetAchievement), new { id = result.Data!.Id }, result.Data);
        }

        /// <summary>
        /// Update an achievement (Admin only)
        /// </summary>
        [HttpPut("achievements/{id}")]
        [ProducesResponseType(typeof(AchievementDto), 200)]
        public async Task<IActionResult> UpdateAchievement(Guid id, [FromBody] UpdateAchievementDto dto)
        {
            var result = await _achievementService.UpdateAchievementAsync(id, dto);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Delete an achievement (Admin only)
        /// </summary>
        [HttpDelete("achievements/{id}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteAchievement(Guid id)
        {
            var result = await _achievementService.DeleteAchievementAsync(id);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return NoContent();
        }

        /// <summary>
        /// Activate or deactivate an achievement (Admin only)
        /// </summary>
        [HttpPost("achievements/{id}/activate")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ToggleAchievementStatus(Guid id, [FromBody] ToggleStatusRequest request)
        {
            var result = await _achievementService.ToggleAchievementStatusAsync(id, request.IsActive);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(new { message = $"Achievement {(request.IsActive ? "activated" : "deactivated")} successfully" });
        }

        /// <summary>
        /// Get achievement by ID (Admin only)
        /// </summary>
        [HttpGet("achievements/{id}")]
        [ProducesResponseType(typeof(AchievementDto), 200)]
        public async Task<IActionResult> GetAchievement(Guid id)
        {
            var result = await _achievementService.GetAchievementByIdAsync(id);
            if (!result.Succeeded)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Data);
        }

        #endregion

        #region Shop Management

        /// <summary>
        /// Create a shop item (Admin only)
        /// </summary>
        [HttpPost("shop/items")]
        [ProducesResponseType(typeof(ShopItemDto), 201)]
        public async Task<IActionResult> CreateShopItem([FromBody] CreateShopItemDto dto)
        {
            var result = await _shopService.CreateShopItemAsync(dto);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return CreatedAtAction(nameof(GetShopItem), new { id = result.Data!.Id }, result.Data);
        }

        /// <summary>
        /// Update a shop item (Admin only)
        /// </summary>
        [HttpPut("shop/items/{id}")]
        [ProducesResponseType(typeof(ShopItemDto), 200)]
        public async Task<IActionResult> UpdateShopItem(Guid id, [FromBody] UpdateShopItemDto dto)
        {
            var result = await _shopService.UpdateShopItemAsync(id, dto);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Delete a shop item (Admin only)
        /// </summary>
        [HttpDelete("shop/items/{id}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteShopItem(Guid id)
        {
            var result = await _shopService.DeleteShopItemAsync(id);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return NoContent();
        }

        /// <summary>
        /// Get shop item by ID (Admin only)
        /// </summary>
        [HttpGet("shop/items/{id}")]
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

        #endregion
    }

    public class AddRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }

    public class LockUserRequest
    {
        public bool IsLocked { get; set; }
    }

    public class ToggleStatusRequest
    {
        public bool IsActive { get; set; }
    }
}

