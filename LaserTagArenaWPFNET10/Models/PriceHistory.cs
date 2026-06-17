using LaserTagArenaWPFNET10.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaserTagArenaWPFNET10.Models
{
    [Table("PriceHistory")]
    public class PriceHistory
    {
        [Key]
        public int HistoryID { get; set; }

        public int EquipmentID { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime ChangeDate { get; set; }
        public int? ChangedByUserID { get; set; }

        [ForeignKey(nameof(EquipmentID))]
        public Equipment? Equipment { get; set; }

        [ForeignKey(nameof(ChangedByUserID))]
        public User? ChangedByUser { get; set; }

        [NotMapped]
        public string? EquipmentName => Equipment?.Name;

        [NotMapped]
        public string? EquipmentSKU => Equipment?.SKU;

        [NotMapped]
        public string ChangedByUserName => ChangedByUser != null
            ? $"{ChangedByUser.LastName} {ChangedByUser.FirstName}"
            : "Система";

        [NotMapped]
        public string OldPriceDisplay => OldPrice?.ToString("N2") + " ₽" ?? "—";

        [NotMapped]
        public string NewPriceDisplay => $"{NewPrice:N2} ₽";

        [NotMapped]
        public string ChangeDateDisplay => ChangeDate.ToString("dd.MM.yyyy HH:mm");
    }
}