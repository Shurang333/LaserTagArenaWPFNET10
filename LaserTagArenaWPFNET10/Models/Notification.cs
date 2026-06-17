using LaserTagArenaWPFNET10.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaserTagArenaWPFNET10.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        public int UserID { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Type { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(UserID))]
        public User? User { get; set; }

        [NotMapped]
        public string CreatedAtDisplay => CreatedAt.ToString("dd.MM.yyyy HH:mm");

        [NotMapped]
        public string TypeDisplay => NotificationTypes.GetLabel(Type);
    }
}
