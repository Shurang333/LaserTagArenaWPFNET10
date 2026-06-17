using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaserTagArenaWPFNET10.Models
{
    [Table("ShipmentStatuses")]
    public class ShipmentStatus
    {
        [Key]
        public int StatusID { get; set; }

        [Required]
        [StringLength(50)]
        public string StatusName { get; set; } = string.Empty;
    }
}