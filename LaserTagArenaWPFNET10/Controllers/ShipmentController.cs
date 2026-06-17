using LaserTagArenaWPFNET10.Data;
using LaserTagArenaWPFNET10.Helpers;
using LaserTagArenaWPFNET10.Models;
using LaserTagArenaWPFNET10.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaserTagArenaWPFNET10.Controllers
{
    public class ShipmentCompleteResult
    {
        public bool Success { get; set; }
        public int ItemsUpdated { get; set; }
        public string? ShipmentNumber { get; set; }
        public List<string> StockLines { get; set; } = new List<string>();
    }

    public class ShipmentController
    {
        private readonly LaserTagDbContext _db;
        private readonly NotificationService _notifications;

        public ShipmentController(LaserTagDbContext db, NotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public List<Shipment> GetAll(string sortBy = "DateDesc", int? statusId = null, string? searchText = null)
        {
            ApplicationSession.RequireAdmin();
            var query = _db.Shipments
                .Include(s => s.Supplier)
                .Include(s => s.Status)
                .AsQueryable();

            if (statusId.HasValue && statusId.Value > 0)
                query = query.Where(s => s.StatusID == statusId.Value);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string s = searchText.Trim();
                query = query.Where(sh =>
                    sh.ShipmentNumber.Contains(s) ||
                    sh.Supplier!.Name.Contains(s));
            }

            return ApplySort(query, sortBy).ToList();
        }

        private static IQueryable<Shipment> ApplySort(IQueryable<Shipment> query, string sortBy)
        {
            return sortBy switch
            {
                "DateAsc" => query.OrderBy(s => s.ShipmentDate),
                "AmountDesc" => query.OrderByDescending(s => s.TotalCost),
                "AmountAsc" => query.OrderBy(s => s.TotalCost),
                "SupplierAsc" => query.OrderBy(s => s.Supplier!.Name),
                "SupplierDesc" => query.OrderByDescending(s => s.Supplier!.Name),
                _ => query.OrderByDescending(s => s.ShipmentDate)
            };
        }

        public bool Add(Shipment shipment)
        {
            ApplicationSession.RequireAdmin();
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                shipment.ShipmentNumber = $"SHP-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
                shipment.StatusID = 1;
                shipment.CreatedAt = DateTime.Now;
                shipment.CreatedByUserID = ApplicationSession.CurrentUser?.UserID ?? 1;

                var items = shipment.Items.ToList();
                shipment.Items = new List<ShipmentItem>();

                _db.Shipments.Add(shipment);
                _db.SaveChanges();

                foreach (var item in items)
                {
                    item.ShipmentID = shipment.ShipmentID;
                    _db.ShipmentItems.Add(item);
                }

                _db.SaveChanges();
                transaction.Commit();

                string supplierName = _db.Suppliers.Find(shipment.SupplierID)?.Name ?? "—";
                _notifications.NotifyStaff(
                    $"Создана поставка {shipment.ShipmentNumber} от «{supplierName}» на {shipment.TotalCost:N0} ₽",
                    NotificationTypes.ShipmentCreate);

                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public ShipmentCompleteResult Complete(int shipmentId)
        {
            ApplicationSession.RequireAdmin();
            var result = new ShipmentCompleteResult();

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var shipment = _db.Shipments
                    .Include(s => s.Supplier)
                    .FirstOrDefault(s => s.ShipmentID == shipmentId);

                if (shipment == null || shipment.StatusID == 3)
                    return result;

                shipment.StatusID = 3;
                shipment.ActualDeliveryDate = DateTime.Now;
                result.ShipmentNumber = shipment.ShipmentNumber;

                var items = _db.ShipmentItems
                    .Include(i => i.Equipment)
                    .Where(i => i.ShipmentID == shipmentId)
                    .ToList();

                foreach (var item in items)
                {
                    var equipment = item.Equipment ?? _db.Equipment.Find(item.EquipmentID);
                    if (equipment == null) continue;

                    int oldStock = equipment.StockQuantity;
                    StockHelper.ApplyStockDelta(equipment, item.Quantity);

                    result.StockLines.Add(StockHelper.FormatStockLine(
                        equipment.Name, item.Quantity, equipment.StockQuantity));

                    _notifications.NotifyStaff(
                        $"Склад: «{equipment.Name}» {oldStock} → {equipment.StockQuantity} шт. (поставка {shipment.ShipmentNumber})",
                        NotificationTypes.StockChange);
                }

                _db.SaveChanges();
                transaction.Commit();

                result.Success = true;
                result.ItemsUpdated = items.Count;

                var summary = new StringBuilder();
                summary.Append($"Поставка {shipment.ShipmentNumber} завершена. ");
                if (result.StockLines.Any())
                    summary.Append(string.Join("; ", result.StockLines.Take(3)));
                if (result.StockLines.Count > 3)
                    summary.Append($" и ещё {result.StockLines.Count - 3} поз.");

                _notifications.NotifyStaff(summary.ToString(), NotificationTypes.ShipmentComplete);

                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Supplier> GetSuppliers()
        {
            ApplicationSession.RequireAdmin();
            return _db.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToList();
        }
    }
}