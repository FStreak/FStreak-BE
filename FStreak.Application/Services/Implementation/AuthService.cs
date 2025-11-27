using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using FStreak.Domain.Entities;
using FStreak.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FStreak.Application.Services.Interface;
using FStreak.Application.DTOs;

namespace FStreak.Application.Services.Implementation
{

    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly FStreakDbContext _context;
        private readonly IAchievementService _achievementService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configuration,
            FStreakDbContext context,
            IAchievementService achievementService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _achievementService = achievementService;
        }

        public async Task<(bool Succeeded, string Message, AuthResult Result)> RegisterUserAsync(
            string email, string username, string firstName, string lastName, string password, string role)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return (false, "Email already exists", null);
            }

            var user = new ApplicationUser
            {
                Email = email,
                UserName = username,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true //auto-confirm emails
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);
            }


            // Gán role cho user (ưu tiên role truyền vào, mặc định là "User")
            var roleToAssign = string.IsNullOrWhiteSpace(role) ? "User" : role;
            if (!await _roleManager.RoleExistsAsync(roleToAssign))
            {
                var roleResult = await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleToAssign,
                    Description = $"Auto-created role: {roleToAssign}"
                });
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user); // Rollback user creation
                    return (false, $"Error creating role {roleToAssign}", null);
                }
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
            if (!addToRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user); // Rollback user creation
                return (false, $"Error assigning user role {roleToAssign}", null);
            }

            // Auto gán achievement "Chào mừng" khi đăng ký
            Guid welcomeAchievementId = Guid.Parse("7c437fc2-1843-4b8c-847c-b3353ea5161b");
            await _achievementService.ClaimAchievementAsync(user.Id, welcomeAchievementId);

            var authResult = await GenerateAuthenticationResultAsync(user);
            return (true, "User registered successfully", authResult);
        }

        public async Task<(bool Succeeded, string Message, AuthResult Result)> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return (false, "Invalid email or password", null);
            }

            var result = await _userManager.CheckPasswordAsync(user, password);
            if (!result)
            {
                return (false, "Invalid email or password", null);
            }

            var authResult = await GenerateAuthenticationResultAsync(user);
            return (true, "Login successful", authResult);
        }

        public async Task<(bool Succeeded, string Message, AuthResult Result)> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return (false, "Invalid access token", null);
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found", null);
            }

            var storedRefreshToken = await _context.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

            if (storedRefreshToken == null)
            {
                return (false, "Invalid refresh token", null);
            }

            if (!storedRefreshToken.IsActive)
            {
                return (false, "Refresh token has been used or revoked", null);
            }

            // Revoke the current refresh token
            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate new tokens
            var authResult = await GenerateAuthenticationResultAsync(user);
            return (true, "Token refreshed successfully", authResult);
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var storedRefreshToken = await _context.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedRefreshToken == null)
            {
                return false;
            }

            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // return true   TODO: implement send token via email
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(false);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private async Task<AuthResult> GenerateAuthenticationResultAsync(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryInMinutes"])),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = GenerateRefreshToken();

            await SaveRefreshTokenAsync(user.Id, refreshToken);

            return new AuthResult
            {
                AccessToken = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken,
                ExpiresIn = int.Parse(_configuration["Jwt:ExpiryInMinutes"]) * 60,
                User = user
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task SaveRefreshTokenAsync(string userId, string token)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7), // Refresh token valid for 7 days
                ReasonRevoked = string.Empty,
                ReplacedByToken = string.Empty
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"])
                ),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = false // This is important for refresh tokens
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}