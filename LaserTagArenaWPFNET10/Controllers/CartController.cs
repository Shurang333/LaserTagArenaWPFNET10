using LaserTagArenaWPFNET10.Services;
using LaserTagArenaWPFNET10.Data;
using LaserTagArenaWPFNET10.Helpers;
using LaserTagArenaWPFNET10.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationException = LaserTagArenaWPFNET10.Helpers.ValidationException;

namespace LaserTagArenaWPFNET10.Controllers
{
    public class CartController
    {
        private readonly LaserTagDbContext _db;
        private readonly NotificationService _notifications;

        public CartController(LaserTagDbContext db, NotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public List<CartItem> GetCart(int userId)
        {
            if (userId <= 0)
                throw new ValidationException("Для просмотра корзины необходимо войти в систему.");

            try
            {
                return _db.CartItems
                    .Include(c => c.Equipment)
                        .ThenInclude(e => e!.Category)
                    .Where(c => c.UserID == userId)
                    .OrderByDescending(c => c.AddedAt)
                    .ToList();
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public void AddToCart(int userId, int equipmentId, int quantity = 1)
        {
            Validators.ValidateCartAdd(userId, equipmentId, quantity).ThrowIfInvalid();

            try
            {
                var equipment = _db.Equipment.Find(equipmentId);
                if (equipment == null)
                    throw new ValidationException("Товар не найден.");

                if (!equipment.IsAvailable)
                    throw new ValidationException($"Товар «{equipment.Name}» недоступен для заказа.");

                if (equipment.StockQuantity <= 0)
                    throw new ValidationException($"Товар «{equipment.Name}» отсутствует на складе.");

                if (quantity > equipment.StockQuantity)
                    throw new ValidationException($"Недостаточно на складе. Доступно: {equipment.StockQuantity} шт.");

                var existing = _db.CartItems.FirstOrDefault(c => c.UserID == userId && c.EquipmentID == equipmentId);
                int newQty = (existing?.Quantity ?? 0) + quantity;
                if (newQty > equipment.StockQuantity)
                    throw new ValidationException($"Нельзя добавить {quantity} шт. — на складе только {equipment.StockQuantity} шт.");

                if (existing != null)
                {
                    existing.Quantity = newQty;
                    existing.AddedAt = DateTime.Now;
                }
                else
                {
                    _db.CartItems.Add(new CartItem
                    {
                        UserID = userId,
                        EquipmentID = equipmentId,
                        Quantity = quantity,
                        AddedAt = DateTime.Now
                    });
                }

                _db.SaveChanges();
                _notifications.Create(userId,
                    $"В корзину: «{equipment.Name}» ×{quantity} ({equipment.CurrentPrice:N0} ₽/шт.)",
                    NotificationTypes.CartAdd);
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public void RemoveFromCart(int cartItemId)
        {
            Validators.ValidateDeleteId(cartItemId, "позиция корзины").ThrowIfInvalid();

            try
            {
                var item = _db.CartItems.Include(c => c.Equipment).FirstOrDefault(c => c.CartItemID == cartItemId);
                if (item == null)
                    throw new ValidationException("Позиция не найдена в корзине.");

                string itemName = item.Equipment?.Name ?? "товар";
                int userId = item.UserID;
                _db.CartItems.Remove(item);
                _db.SaveChanges();

                _notifications.Create(userId,
                    $"Из корзины удалено: «{itemName}»",
                    NotificationTypes.CartRemove);
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public void ClearCart(int userId)
        {
            Validators.ValidateOrderCheckout(userId).ThrowIfInvalid();

            try
            {
                var items = _db.CartItems.Where(c => c.UserID == userId).ToList();
                _db.CartItems.RemoveRange(items);
                _db.SaveChanges();
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }
    }
}