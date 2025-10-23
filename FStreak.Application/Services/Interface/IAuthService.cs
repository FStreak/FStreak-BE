using FStreak.Application.DTOs;

namespace FStreak.Application.Services.Interface
{
    public interface IAuthService
    {
    Task<(bool Succeeded, string Message, AuthResult Result)> RegisterUserAsync(string email, string username, string firstName, string lastName, string password, string role);
        Task<(bool Succeeded, string Message, AuthResult Result)> LoginAsync(string email, string password);
        Task<(bool Succeeded, string Message, AuthResult Result)> RefreshTokenAsync(string accessToken, string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> ValidateTokenAsync(string token);
    }
}
