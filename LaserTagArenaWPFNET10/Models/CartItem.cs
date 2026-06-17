using LaserTagArenaWPFNET10.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaserTagArenaWPFNET10.Models
{
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        public int CartItemID { get; set; }

        public int UserID { get; set; }
        public int EquipmentID { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }

        [ForeignKey(nameof(UserID))]
        public User? User { get; set; }  // nullable для EF Core

        [ForeignKey(nameof(EquipmentID))]
        public Equipment? Equipment { get; set; }  // nullable для EF Core

        [NotMapped]
        public string EquipmentName => Equipment?.Name ?? string.Empty;

        [NotMapped]
        public string EquipmentSKU => Equipment?.SKU ?? string.Empty;

        [NotMapped]
        public decimal PricePerUnit => Equipment?.CurrentPrice ?? 0;

        [NotMapped]
        public decimal TotalPrice => PricePerUnit * Quantity;

        [NotMapped]
        public string TotalPriceDisplay => $"{TotalPrice:N2} ₽";
    }
}