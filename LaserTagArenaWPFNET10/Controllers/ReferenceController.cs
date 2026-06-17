using LaserTagArenaWPFNET10.Data;
using LaserTagArenaWPFNET10.Helpers;
using LaserTagArenaWPFNET10.Models;
using LaserTagArenaWPFNET10.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationException = LaserTagArenaWPFNET10.Helpers.ValidationException;

namespace LaserTagArenaWPFNET10.Controllers
{
    public class ReferenceController
    {
        private readonly LaserTagDbContext _db;
        private readonly NotificationService _notifications;

        public ReferenceController(LaserTagDbContext db, NotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public List<Category> GetCategories()
        {
            try
            {
                return _db.Categories.OrderBy(c => c.Name).ToList();
            }
            catch (Exception ex)
            {
                throw new ValidationException(ExceptionTranslator.Translate(ex));
            }
        }

        public List<Manufacturer> GetManufacturers()
        {
            try
            {
                return _db.Manufacturers.OrderBy(m => m.Name).ToList();
            }
            catch (Exception ex)
            {
                throw new ValidationException(ExceptionTranslator.Translate(ex));
            }
        }

        public void AddCategory(string name, string? description = null)
        {
            Validators.ValidateReferenceName(name, "Название категории").ThrowIfInvalid();

            try
            {
                name = name.Trim();
                if (_db.Categories.Any(c => c.Name == name))
                    throw new ValidationException("Категория с таким названием уже существует.");

                _db.Categories.Add(new Category { Name = name, Description = description?.Trim() });
                _db.SaveChanges();
                _notifications.NotifyStaff($"Добавлена категория «{name}»", NotificationTypes.CategoryAdd);
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public void DeleteCategory(int categoryId)
        {
            Validators.ValidateDeleteId(categoryId, "категория").ThrowIfInvalid();

            try
            {
                var category = _db.Categories.Find(categoryId);
                if (category == null)
                    throw new ValidationException("Категория не найдена.");

                if (_db.Equipment.Any(e => e.CategoryID == categoryId))
                    throw new ValidationException("Невозможно удалить: категория используется в товарах.");

                string catName = category.Name;
                _db.Categories.Remove(category);
                _db.SaveChanges();
                _notifications.NotifyStaff($"Удалена категория «{catName}»", NotificationTypes.CategoryDelete);
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public void AddManufacturer(string name, string? country = null)
        {
            Validators.ValidateReferenceName(name, "Название производителя").ThrowIfInvalid();

            try
            {
                name = name.Trim();
                if (_db.Manufacturers.Any(m => m.Name == name))
                    throw new ValidationException("Производитель с таким названием уже существует.");

                _db.Manufacturers.Add(new Manufacturer { Name = name, Country = country?.Trim() });
                _db.SaveChanges();
                _notifications.NotifyStaff($"Добавлен производитель «{name}»", NotificationTypes.ManufacturerAdd);
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public void DeleteManufacturer(int manufacturerId)
        {
            Validators.ValidateDeleteId(manufacturerId, "производитель").ThrowIfInvalid();

            try
            {
                var manufacturer = _db.Manufacturers.Find(manufacturerId);
                if (manufacturer == null)
                    throw new ValidationException("Производитель не найден.");

                if (_db.Equipment.Any(e => e.ManufacturerID == manufacturerId))
                    throw new ValidationException("Невозможно удалить: производитель указан в товарах.");

                string mName = manufacturer.Name;
                _db.Manufacturers.Remove(manufacturer);
                _db.SaveChanges();
                _notifications.NotifyStaff($"Удалён производитель «{mName}»", NotificationTypes.ManufacturerDelete);
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }
    }
}