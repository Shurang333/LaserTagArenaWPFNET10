using LaserTagArenaWPFNET10.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaserTagArenaWPFNET10.Models
{
    [Table("Shipments")]
    public class Shipment
    {
        [Key]
        public int ShipmentID { get; set; }

        [Required]
        [StringLength(50)]
        public string ShipmentNumber { get; set; } = string.Empty;

        public int SupplierID { get; set; }
        public DateTime ShipmentDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public int StatusID { get; set; }
        public decimal? TotalCost { get; set; }
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedAt { get; set; }

        [ForeignKey(nameof(SupplierID))]
        public Supplier? Supplier { get; set; }

        [ForeignKey(nameof(StatusID))]
        public ShipmentStatus? Status { get; set; }

        public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();

        [NotMapped]
        public string? SupplierName => Supplier?.Name;

        [NotMapped]
        public string? StatusName => Status?.StatusName;

        [NotMapped]
        public string ShipmentDateDisplay => ShipmentDate.ToString("dd.MM.yyyy");

        [NotMapped]
        public string TotalCostDisplay => TotalCost?.ToString("N2") + " ₽" ?? "—";
    }

    [Table("ShipmentItems")]
    public class ShipmentItem
    {
        [Key]
        public int ShipmentItemID { get; set; }

        public int ShipmentID { get; set; }
        public int EquipmentID { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }

        [ForeignKey(nameof(ShipmentID))]
        public Shipment? Shipment { get; set; }

        [ForeignKey(nameof(EquipmentID))]
        public Equipment? Equipment { get; set; }

        [NotMapped]
        public string? EquipmentName => Equipment?.Name;

        [NotMapped]
        public string? EquipmentSKU => Equipment?.SKU;

        [NotMapped]
        public decimal TotalPrice => Quantity * PricePerUnit;
    }
}