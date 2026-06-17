using LaserTagArenaWPFNET10.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaserTagArenaWPFNET10.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        public int UserID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? GameDate { get; set; }
        public TimeSpan? GameTime { get; set; }
        public int? ParticipantsCount { get; set; }
        public int StatusID { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? CompletionDate { get; set; }

        [ForeignKey(nameof(UserID))]
        public User? User { get; set; }

        [ForeignKey(nameof(StatusID))]
        public OrderStatus? Status { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        [NotMapped]
        public string CustomerName => User != null ? $"{User.LastName} {User.FirstName}" : string.Empty;

        [NotMapped]
        public string? CustomerPhone => User?.Phone;

        [NotMapped]
        public string? StatusName => Status?.StatusName;

        [NotMapped]
        public string? GameTimeDisplay => GameTime?.ToString(@"hh\:mm");

        [NotMapped]
        public string OrderDateDisplay => OrderDate.ToString("dd.MM.yyyy HH:mm");

        [NotMapped]
        public string TotalAmountDisplay => $"{TotalAmount:N2} ₽";
    }

    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        public int OrderItemID { get; set; }

        public int OrderID { get; set; }
        public int EquipmentID { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public int DiscountPercent { get; set; }

        [ForeignKey(nameof(OrderID))]
        public Order? Order { get; set; }

        [ForeignKey(nameof(EquipmentID))]
        public Equipment? Equipment { get; set; }

        [NotMapped]
        public string? EquipmentName => Equipment?.Name;

        [NotMapped]
        public string? EquipmentSKU => Equipment?.SKU;

        [NotMapped]
        public decimal TotalPrice => Quantity * PricePerUnit;

        [NotMapped]
        public string TotalPriceDisplay => $"{TotalPrice:N2} ₽";

        [NotMapped]
        public string PricePerUnitDisplay => $"{PricePerUnit:N2} ₽";
    }
}