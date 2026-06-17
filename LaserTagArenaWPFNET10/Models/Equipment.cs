using LaserTagArenaWPFNET10.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaserTagArenaWPFNET10.Models
{
    [Table("Equipment")]
    public class Equipment
    {
        [Key]
        public int EquipmentID { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public int UnitID { get; set; }
        public decimal? OldPrice { get; set; }
        public int DiscountPercent { get; set; }
        public decimal CurrentPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsVisible { get; set; }
        public string? ImagePath { get; set; }
        public string? Tags { get; set; }
        public int? SupplierID { get; set; }
        public int? ManufacturerID { get; set; }
        public int? CategoryID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(UnitID))]
        public Unit? Unit { get; set; }

        [ForeignKey(nameof(SupplierID))]
        public Supplier? Supplier { get; set; }

        [ForeignKey(nameof(ManufacturerID))]
        public Manufacturer? Manufacturer { get; set; }

        [ForeignKey(nameof(CategoryID))]
        public Category? Category { get; set; }

        [NotMapped]
        public string UnitName => Unit?.Name ?? string.Empty;

        [NotMapped]
        public string SupplierName => Supplier?.Name ?? string.Empty;

        [NotMapped]
        public string ManufacturerName => Manufacturer?.Name ?? string.Empty;

        [NotMapped]
        public string CategoryName => Category?.Name ?? string.Empty;

        [NotMapped]
        public string PriceDisplay
        {
            get
            {
                if (OldPrice.HasValue && DiscountPercent > 0)
                    return $"{CurrentPrice:N2} ₽ (было {OldPrice:N2} ₽, скидка {DiscountPercent}%)";
                return $"{CurrentPrice:N2} ₽";
            }
        }

        [NotMapped]
        public string PriceBackground => CurrentPrice > 1000 ? "#FFD700" : "Transparent";

        [NotMapped]
        public string AvailabilityText
        {
            get
            {
                if (!IsAvailable) return "Нет в наличии";
                if (StockQuantity == 0) return "Ожидается";
                return $"В наличии ({StockQuantity} шт.)";
            }
        }

        [NotMapped]
        public string AvailabilityColor
        {
            get
            {
                if (!IsAvailable) return "#FF4444";
                if (StockQuantity == 0) return "#FFA500";
                return "#4CAF50";
            }
        }
    }
}