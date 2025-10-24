using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FStreak.API.DTOs;
using FStreak.Application.Services.Interface;
using FStreak.Domain.Entities;
using System.Security.Claims;

namespace FStreak.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(IAuthService authService, UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="model">Registration details</param>
        /// <returns>Authentication result including tokens</returns>
        /// <response code="200">Registration successful</response>
        /// <response code="400">Invalid input or user already exists</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(AuthResponseDto), 400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Succeeded = false,
                    Message = "Invalid input"
                });
            }

            var result = await _authService.RegisterUserAsync(
                model.Email,
                model.Username,
                model.FirstName,
                model.LastName,
                model.Password,
                model.Role);

            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDto
                {
                    Succeeded = false,
                    Message = result.Message
                });
            }

            var roles = await _userManager.GetRolesAsync(result.Result.User);
            return Ok(new AuthResponseDto
            {
                Succeeded = true,
                Message = result.Message,
                AccessToken = result.Result.AccessToken,
                RefreshToken = result.Result.RefreshToken,
                ExpiresIn = result.Result.ExpiresIn,
                User = MapToUserDto(result.Result.User, roles)
            });
        }

        /// <summary>
        /// Authenticate user and get tokens
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <returns>Authentication result including tokens</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid credentials</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(AuthResponseDto), 400)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Succeeded = false,
                    Message = "Invalid input"
                });
            }

            var result = await _authService.LoginAsync(model.Email, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDto
                {
                    Succeeded = false,
                    Message = result.Message
                });
            }

            var roles = await _userManager.GetRolesAsync(result.Result.User);
            return Ok(new AuthResponseDto
            {
                Succeeded = true,
                Message = result.Message,
                AccessToken = result.Result.AccessToken,
                RefreshToken = result.Result.RefreshToken,
                ExpiresIn = result.Result.ExpiresIn,
                User = MapToUserDto(result.Result.User, roles)
            });
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="model">Current access and refresh tokens</param>
        /// <returns>New access and refresh tokens</returns>
        /// <response code="200">Token refresh successful</response>
        /// <response code="400">Invalid or expired tokens</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(AuthResponseDto), 400)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto model)
        {
            var result = await _authService.RefreshTokenAsync(model.AccessToken, model.RefreshToken);
            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDto
                {
                    Succeeded = false,
                    Message = result.Message
                });
            }

            var roles = await _userManager.GetRolesAsync(result.Result.User);
            return Ok(new AuthResponseDto
            {
                Succeeded = true,
                Message = result.Message,
                AccessToken = result.Result.AccessToken,
                RefreshToken = result.Result.RefreshToken,
                ExpiresIn = result.Result.ExpiresIn,
                User = MapToUserDto(result.Result.User, roles)
            });
        }

        /// <summary>
        /// Logout user by revoking refresh token
        /// </summary>
        /// <returns>Success status</returns>
        /// <response code="200">Logout successful</response>
        /// <response code="400">Invalid token</response>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(AuthResponseDto), 400)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto model)
        {
            var success = await _authService.RevokeTokenAsync(model.RefreshToken);
            return Ok(new AuthResponseDto
            {
                Succeeded = success,
                Message = success ? "Logged out successfully" : "Invalid token"
            });
        }

        /// <summary>
        /// Request password reset email
        /// </summary>
        /// <param name="model">User's email</param>
        /// <returns>Success status</returns>
        /// <response code="200">Password reset email sent</response>
        /// <response code="400">Email not found</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(AuthResponseDto), 400)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto model)
        {
            var result = await _authService.ForgotPasswordAsync(model.Email);
            return Ok(new AuthResponseDto
            {
                Succeeded = result,
                Message = result ? "Password reset email sent" : "Email not found"
            });
        }

        /// <summary>
        /// Reset password using token
        /// </summary>
        /// <param name="model">Password reset details</param>
        /// <returns>Success status</returns>
        /// <response code="200">Password reset successful</response>
        /// <response code="400">Invalid token or password</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(AuthResponseDto), 400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto model)
        {
            var result = await _authService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
            return Ok(new AuthResponseDto
            {
                Succeeded = result,
                Message = result ? "Password reset successful" : "Invalid token or password"
            });
        }

        /// <summary>
        /// Get current user's profile
        /// </summary>
        /// <returns>User profile information</returns>
        /// <response code="200">Profile retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(MapToUserDto(user, roles));
        }

        private static UserDto MapToUserDto(ApplicationUser user, IList<string> roles)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CurrentStreak = user.CurrentStreak,
                LongestStreak = user.LongestStreak,
                Roles = roles.ToArray(),
            };
        }
    }
}