using FStreak.Domain.Entities;

namespace FStreak.Application.DTOs
{
    public class AuthResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public ApplicationUser User { get; set; }
    }
}