using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class Reaction : BaseEntity
    {
        [Key]
        public int ReactionId { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string RecipientId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; }

        // Navigation properties
        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; }

        [ForeignKey("RecipientId")]
        public virtual ApplicationUser Recipient { get; set; }
    }
}