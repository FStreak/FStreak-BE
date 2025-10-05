using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class UserFriend : BaseEntity
    {
        [Key]
        public int UserFriendId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string FriendId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "PENDING";

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("FriendId")]
        public virtual ApplicationUser Friend { get; set; }
    }
}