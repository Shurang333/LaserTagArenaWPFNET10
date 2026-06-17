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
    public class EquipmentController
    {
        private readonly LaserTagDbContext _db;
        private readonly NotificationService _notifications;

        public EquipmentController(LaserTagDbContext db, NotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public List<Equipment> GetAll(string sortBy = "NameAsc", string? searchText = null, string? tagFilter = null)
        {
            Validators.ValidateSearchText(searchText).ThrowIfInvalid();
            Validators.ValidateSearchText(tagFilter, 200).ThrowIfInvalid();

            try
            {
                var query = _db.Equipment
                    .Include(e => e.Unit)
                    .Include(e => e.Supplier)
                    .Include(e => e.Manufacturer)
                    .Include(e => e.Category)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string s = searchText.ToLower();
                    query = query.Where(e => e.Name.ToLower().Contains(s) || e.SKU.ToLower().Contains(s));
                }

                if (!string.IsNullOrWhiteSpace(tagFilter))
                {
                    string t = tagFilter.ToLower();
                    query = query.Where(e => e.Tags != null && e.Tags.ToLower().Contains(t));
                }

                return ApplySort(query, sortBy).ToList();
            }
            catch (Exception ex)
            {
                throw new ValidationException(ExceptionTranslator.Translate(ex));
            }
        }

        public void Add(Equipment equipment)
        {
            Validators.ValidateEquipment(equipment, isNew: true).ThrowIfInvalid();

            try
            {
                equipment.SKU = equipment.SKU.Trim();
                equipment.Name = equipment.Name.Trim();

                if (_db.Equipment.Any(e => e.SKU == equipment.SKU))
                    throw new ValidationException("Товар с таким артикулом уже существует.");

                if (!_db.Units.Any(u => u.UnitID == (equipment.UnitID > 0 ? equipment.UnitID : 1)))
                    throw new ValidationException("Единица измерения не найдена в базе.");

                equipment.CreatedAt = DateTime.Now;
                equipment.IsVisible = true;
                if (equipment.UnitID == 0) equipment.UnitID = 1;

                _db.Equipment.Add(equipment);
                _db.SaveChanges();
                SavePriceHistory(equipment.EquipmentID, null, equipment.CurrentPrice);

                _notifications.NotifyStaff(
                    $"Добавлен товар «{equipment.Name}» ({equipment.SKU}), цена {equipment.CurrentPrice:N0} ₽",
                    NotificationTypes.EquipmentAdd);
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public void Update(Equipment equipment)
        {
            Validators.ValidateEquipment(equipment, isNew: false).ThrowIfInvalid();

            try
            {
                var existing = _db.Equipment.Find(equipment.EquipmentID);
                if (existing == null)
                    throw new ValidationException("Товар не найден. Возможно, он был удалён.");

                equipment.SKU = equipment.SKU.Trim();
                if (_db.Equipment.Any(e => e.SKU == equipment.SKU && e.EquipmentID != equipment.EquipmentID))
                    throw new ValidationException("Товар с таким артикулом уже существует.");

                decimal oldPrice = existing.CurrentPrice;
                int oldStock = existing.StockQuantity;
                existing.SKU = equipment.SKU;
                existing.Name = equipment.Name.Trim();
                existing.Description = equipment.Description;
                existing.OldPrice = equipment.OldPrice;
                existing.DiscountPercent = equipment.DiscountPercent;
                existing.CurrentPrice = equipment.CurrentPrice;
                existing.StockQuantity = equipment.StockQuantity;
                existing.IsAvailable = equipment.IsAvailable;
                existing.ImagePath = equipment.ImagePath;
                existing.Tags = equipment.Tags;
                existing.ManufacturerID = equipment.ManufacturerID;
                existing.CategoryID = equipment.CategoryID;
                existing.UpdatedAt = DateTime.Now;

                _db.SaveChanges();

                if (oldPrice != equipment.CurrentPrice)
                {
                    SavePriceHistory(equipment.EquipmentID, oldPrice, equipment.CurrentPrice);
                    _notifications.NotifyStaff(
                        $"Цена «{existing.Name}»: {oldPrice:N0} → {equipment.CurrentPrice:N0} ₽",
                        NotificationTypes.PriceChange);
                }

                if (oldStock != equipment.StockQuantity)
                {
                    _notifications.NotifyStaff(
                        $"Остаток «{existing.Name}»: {oldStock} → {equipment.StockQuantity} шт.",
                        NotificationTypes.StockChange);
                }

                _notifications.NotifyStaff($"Обновлён товар «{existing.Name}»", NotificationTypes.EquipmentUpdate);
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public void Delete(int equipmentId)
        {
            ApplicationSession.RequireAdmin();
            Validators.ValidateDeleteId(equipmentId, "товар").ThrowIfInvalid();

            try
            {
                var equipment = _db.Equipment.Find(equipmentId);
                if (equipment == null)
                    throw new ValidationException("Товар не найден.");

                string name = equipment.Name;
                _db.Equipment.Remove(equipment);
                _db.SaveChanges();

                _notifications.NotifyStaff($"Удалён товар «{name}»", NotificationTypes.EquipmentDelete);
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        private static IQueryable<Equipment> ApplySort(IQueryable<Equipment> query, string sortBy)
        {
            return sortBy switch
            {
                "NameDesc" => query.OrderByDescending(e => e.Name),
                "PriceAsc" => query.OrderBy(e => e.CurrentPrice),
                "PriceDesc" => query.OrderByDescending(e => e.CurrentPrice),
                "StockAsc" => query.OrderBy(e => e.StockQuantity),
                "StockDesc" => query.OrderByDescending(e => e.StockQuantity),
                _ => query.OrderBy(e => e.Name)
            };
        }

        private void SavePriceHistory(int equipmentId, decimal? oldPrice, decimal newPrice)
        {
            _db.PriceHistories.Add(new PriceHistory
            {
                EquipmentID = equipmentId,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                ChangeDate = DateTime.Now,
                ChangedByUserID = ApplicationSession.CurrentUser?.UserID
            });
            _db.SaveChanges();
        }
    }
}