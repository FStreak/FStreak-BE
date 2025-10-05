using Microsoft.AspNetCore.Identity;

namespace FStreak.Domain.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
    }
}