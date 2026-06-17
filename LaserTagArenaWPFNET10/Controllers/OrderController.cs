using LaserTagArenaWPFNET10.Data;
using LaserTagArenaWPFNET10.Helpers;
using LaserTagArenaWPFNET10.Models;
using LaserTagArenaWPFNET10.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationException = LaserTagArenaWPFNET10.Helpers.ValidationException;

namespace LaserTagArenaWPFNET10.Controllers
{
    public class OrderController
    {
        private readonly LaserTagDbContext _db;
        private readonly NotificationService _notifications;
        private readonly PdfReceiptService _pdfService;

        public OrderController(LaserTagDbContext db, NotificationService notifications, PdfReceiptService pdfService)
        {
            _db = db;
            _notifications = notifications;
            _pdfService = pdfService;
        }

        public List<Order> GetOrders(int? statusId = null, string? searchText = null, string sortBy = "DateDesc")
        {
            Validators.ValidateSearchText(searchText).ThrowIfInvalid();
            try
            {
                var query = _db.Orders
                    .Include(o => o.User)
                    .Include(o => o.Status)
                    .AsQueryable();

                if (statusId.HasValue && statusId.Value > 0)
                    query = query.Where(o => o.StatusID == statusId.Value);

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(o =>
                        o.OrderNumber.Contains(searchText) ||
                        o.User!.Phone.Contains(searchText) ||
                        o.User.LastName.Contains(searchText) ||
                        o.User.FirstName.Contains(searchText));
                }

                return ApplySort(query, sortBy).ToList();
            }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public Order? GetById(int orderId)
        {
            return _db.Orders
                .Include(o => o.User)
                .Include(o => o.Status)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Equipment)
                .FirstOrDefault(o => o.OrderID == orderId);
        }

        public Order CreateFromCart(int userId, DateTime? gameDate, string? paymentMethod)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var cartItems = _db.CartItems
                    .Include(c => c.Equipment)
                    .Where(c => c.UserID == userId)
                    .ToList();

                if (!cartItems.Any())
                    throw new ValidationException("Корзина пуста. Добавьте товары перед оформлением заказа.");

                var order = new Order
                {
                    OrderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                    UserID = userId,
                    OrderDate = DateTime.Now,
                    GameDate = gameDate,
                    StatusID = 1,
                    PaymentMethod = paymentMethod,
                    TotalAmount = cartItems.Sum(c => c.Equipment!.CurrentPrice * c.Quantity)
                };

                _db.Orders.Add(order);
                _db.SaveChanges();

                foreach (var cart in cartItems)
                {
                    _db.OrderItems.Add(new OrderItem
                    {
                        OrderID = order.OrderID,
                        EquipmentID = cart.EquipmentID,
                        Quantity = cart.Quantity,
                        PricePerUnit = cart.Equipment!.CurrentPrice,
                        DiscountPercent = cart.Equipment.DiscountPercent
                    });

                    var equipment = _db.Equipment.Find(cart.EquipmentID);
                    if (equipment != null)
                    {
                        int oldStock = equipment.StockQuantity;
                        StockHelper.ApplyStockDelta(equipment, -cart.Quantity);
                        _notifications.Create(userId,
                            $"Списано со склада: «{equipment.Name}» {oldStock} → {equipment.StockQuantity} шт. (заказ {order.OrderNumber})",
                            NotificationTypes.StockChange);
                    }
                }

                _db.CartItems.RemoveRange(cartItems);
                _db.SaveChanges();
                transaction.Commit();

                _notifications.Create(userId, $"Создан заказ {order.OrderNumber} на {order.TotalAmount:N0} ₽", NotificationTypes.OrderCreate);
                _notifications.NotifyAdmins($"Новый заказ {order.OrderNumber} от {ApplicationSession.CurrentUser?.FullName}", NotificationTypes.OrderCreate);
                return GetById(order.OrderID)!;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public Order CreateManual(int userId, List<OrderItem> items, DateTime? gameDate, string? paymentMethod)
        {
            Validators.ValidateOrderCheckout(userId).ThrowIfInvalid();

            if (items == null || !items.Any())
                throw new ValidationException("Добавьте хотя бы одну позицию в заказ.");

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                decimal total = 0;
                foreach (var item in items)
                {
                    var eq = _db.Equipment.Find(item.EquipmentID);
                    if (eq == null) throw new ValidationException("Один из выбранных товаров не найден.");
                    item.PricePerUnit = eq.CurrentPrice;
                    item.DiscountPercent = eq.DiscountPercent;
                    total += item.PricePerUnit * item.Quantity;
                }

                var order = new Order
                {
                    OrderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                    UserID = userId,
                    OrderDate = DateTime.Now,
                    GameDate = gameDate,
                    StatusID = 1,
                    PaymentMethod = paymentMethod,
                    TotalAmount = total
                };

                _db.Orders.Add(order);
                _db.SaveChanges();

                foreach (var item in items)
                {
                    item.OrderID = order.OrderID;
                    _db.OrderItems.Add(item);

                    var equipment = _db.Equipment.Find(item.EquipmentID);
                    if (equipment != null)
                    {
                        int oldStock = equipment.StockQuantity;
                        StockHelper.ApplyStockDelta(equipment, -item.Quantity);
                        _notifications.Create(userId,
                            $"Списано: «{equipment.Name}» {oldStock} → {equipment.StockQuantity} шт.",
                            NotificationTypes.StockChange);
                    }
                }

                _db.SaveChanges();
                transaction.Commit();

                _notifications.Create(userId, $"Создан заказ {order.OrderNumber} на {total:N0} ₽", NotificationTypes.OrderCreate);
                return GetById(order.OrderID)!;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool UpdateStatus(int orderId, int newStatusId)
        {
            Validators.ValidateDeleteId(orderId, "заказ").ThrowIfInvalid();
            try
            {
                var order = _db.Orders.Include(o => o.Status).FirstOrDefault(o => o.OrderID == orderId);
                if (order == null)
                    throw new ValidationException("Заказ не найден.");

                string oldStatus = order.Status?.StatusName ?? "—";
                order.StatusID = newStatusId;
                if (newStatusId == 5 || newStatusId == 7)
                    order.CompletionDate = DateTime.Now;

                _db.SaveChanges();

                var newStatus = _db.OrderStatuses.Find(newStatusId)?.StatusName ?? "—";
                _notifications.NotifyStaff(
                    $"Заказ {order.OrderNumber}: {oldStatus} → {newStatus}",
                    NotificationTypes.OrderStatus);

                return true;
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public void Delete(int orderId)
        {
            ApplicationSession.RequireAdmin();
            Validators.ValidateDeleteId(orderId, "заказ").ThrowIfInvalid();
            try
            {
                var order = _db.Orders.Include(o => o.Items).FirstOrDefault(o => o.OrderID == orderId);
                if (order == null)
                    throw new ValidationException("Заказ не найден.");

                _db.OrderItems.RemoveRange(order.Items);
                _db.Orders.Remove(order);
                _db.SaveChanges();

                _notifications.NotifyStaff($"Удалён заказ {order.OrderNumber}", NotificationTypes.OrderDelete);
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public string GeneratePdfReceipt(int orderId)
        {
            Validators.ValidateDeleteId(orderId, "заказ").ThrowIfInvalid();
            var order = GetById(orderId);
            if (order == null)
                throw new ValidationException("Заказ не найден. Невозможно сформировать чек.");
            return _pdfService.GenerateOrderReceipt(order);
        }

        public List<KeyValuePair<int, string>> GetOrderStatuses()
        {
            return _db.OrderStatuses
                .OrderBy(s => s.StatusID)
                .Select(s => new KeyValuePair<int, string>(s.StatusID, s.StatusName))
                .ToList();
        }

        private static IQueryable<Order> ApplySort(IQueryable<Order> query, string sortBy)
        {
            return sortBy switch
            {
                "DateAsc" => query.OrderBy(o => o.OrderDate),
                "AmountAsc" => query.OrderBy(o => o.TotalAmount),
                "AmountDesc" => query.OrderByDescending(o => o.TotalAmount),
                _ => query.OrderByDescending(o => o.OrderDate)
            };
        }
    }
}