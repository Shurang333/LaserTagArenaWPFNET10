using LaserTagArenaWPFNET10.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;

namespace LaserTagArenaWPFNET10.Data
{
    /// <summary>
    /// Инициализация базы данных для EF Core.
    /// </summary>
    public static class LaserTagDatabaseInitializer
    {
        private const string ChiefAdminEmail = "chief@lasertag.ru";
        private const string ChiefAdminPasswordHash =
            "ca5dd000b260e9d6c3c5e03e7fa3a85c9b803a7898dae3a13b206341a04f6067";

        private const string DefaultAdminEmail = "admin@lasertag.ru";
        private const string DefaultAdminPasswordHash =
            "240be518fabd2724ddb6f04eeb1da596744acd19ca29a283a3350d8d2a3cc8e";

        public static void Initialize()
        {
            using (var db = new LaserTagDbContext(
                new DbContextOptionsBuilder<LaserTagDbContext>()
                    .UseSqlServer(App.Configuration!.GetConnectionString("LaserTagDB"))  // <-- ИСПРАВЛЕНО
                    .Options))
            {
                db.Database.EnsureCreated();
                SeedReferenceData(db);
            }
        }

        private static void SeedReferenceData(LaserTagDbContext context)
        {
            // Роли
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleName = "Администратор" },
                    new Role { RoleName = "Клиент" },
                    new Role { RoleName = "Менеджер" },
                    new Role { RoleName = "Админ сайта" }
                );
            }

            // Единицы измерения
            if (!context.Units.Any())
                context.Units.Add(new Unit { Name = "шт." });

            // Категории
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Штурмовое" },
                    new Category { Name = "Пистолеты" },
                    new Category { Name = "Снайперское" }
                );
            }

            // Производители
            if (!context.Manufacturers.Any())
            {
                context.Manufacturers.AddRange(
                    new Manufacturer { Name = "LaserWar", Country = "Россия" },
                    new Manufacturer { Name = "LASERMAX", Country = "Россия" }
                );
            }

            // Поставщики
            if (!context.Suppliers.Any())
            {
                context.Suppliers.AddRange(
                    new Supplier { Name = "ООО ЛазерТех", IsActive = true },
                    new Supplier { Name = "ИП Иванов", IsActive = true }
                );
            }

            // Статусы заказов
            if (!context.OrderStatuses.Any())
            {
                context.OrderStatuses.AddRange(
                    new OrderStatus { StatusName = "Новый" },
                    new OrderStatus { StatusName = "В обработке" },
                    new OrderStatus { StatusName = "Оплачен" },
                    new OrderStatus { StatusName = "Отменен" },
                    new OrderStatus { StatusName = "Выдан" },
                    new OrderStatus { StatusName = "Возврат" },
                    new OrderStatus { StatusName = "Завершен" }
                );
            }

            // Статусы поставок
            if (!context.ShipmentStatuses.Any())
            {
                context.ShipmentStatuses.AddRange(
                    new ShipmentStatus { StatusName = "Ожидается" },
                    new ShipmentStatus { StatusName = "В пути" },
                    new ShipmentStatus { StatusName = "Доставлено" }
                );
            }

            // Сохраняем справочники
            context.SaveChanges();

            // Добавляем пользователей
            SeedDefaultUsers(context);

            // Добавляем товары, если их нет
            if (!context.Equipment.Any())
            {
                var unit = context.Units.First();
                var manufacturer1 = context.Manufacturers.First();
                var manufacturer2 = context.Manufacturers.Skip(1).First();
                var category1 = context.Categories.First();
                var category2 = context.Categories.Skip(1).First();
                var category3 = context.Categories.Skip(2).First();

                context.Equipment.AddRange(
                    new Equipment
                    {
                        SKU = "AK-LT-001",
                        Name = "Автомат АК-12 Лазертаг",
                        Description = "Штурмовое оружие",
                        UnitID = unit.UnitID,
                        OldPrice = 45000,
                        DiscountPercent = 10,
                        CurrentPrice = 40500,
                        StockQuantity = 15,
                        IsAvailable = true,
                        IsVisible = true,
                        Tags = "штурм,автомат,ак",
                        ManufacturerID = manufacturer1.ManufacturerID,
                        CategoryID = category1.CategoryID,
                        ImagePath = "/ProductImages/no-image.png",
                        CreatedAt = DateTime.Now
                    },
                    new Equipment
                    {
                        SKU = "PST-LT-001",
                        Name = "Пистолет Glock 17",
                        Description = "Пистолет для CQB",
                        UnitID = unit.UnitID,
                        CurrentPrice = 18000,
                        StockQuantity = 20,
                        IsAvailable = true,
                        IsVisible = true,
                        Tags = "пистолет,cqb",
                        ManufacturerID = manufacturer2.ManufacturerID,
                        CategoryID = category2.CategoryID,
                        ImagePath = "/ProductImages/no-image.png",
                        CreatedAt = DateTime.Now
                    },
                    new Equipment
                    {
                        SKU = "SNP-LT-001",
                        Name = "Снайперская винтовка СВД",
                        Description = "Дальняя стрельба",
                        UnitID = unit.UnitID,
                        OldPrice = 65000,
                        DiscountPercent = 15,
                        CurrentPrice = 55250,
                        StockQuantity = 5,
                        IsAvailable = true,
                        IsVisible = true,
                        Tags = "снайпер,дальняя",
                        ManufacturerID = manufacturer1.ManufacturerID,
                        CategoryID = category3.CategoryID,
                        ImagePath = "/ProductImages/no-image.png",
                        CreatedAt = DateTime.Now
                    }
                );

                context.SaveChanges();
            }
        }

        private static void SeedDefaultUsers(LaserTagDbContext context)
        {
            var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Администратор");
            if (adminRole == null) return;

            if (!context.Users.Any(u => u.Email == DefaultAdminEmail))
            {
                context.Users.Add(new User
                {
                    LastName = "Админов",
                    FirstName = "Админ",
                    Phone = "+79001234567",
                    Email = DefaultAdminEmail,
                    PasswordHash = DefaultAdminPasswordHash,
                    RegistrationDate = DateTime.Now,
                    Status = true,
                    Discount = 0,
                    RoleID = adminRole.RoleID
                });
            }

            if (!context.Users.Any(u => u.Email == ChiefAdminEmail))
            {
                context.Users.Add(new User
                {
                    LastName = "Главный",
                    FirstName = "Администратор",
                    Phone = "79009876543",
                    Email = ChiefAdminEmail,
                    PasswordHash = ChiefAdminPasswordHash,
                    RegistrationDate = DateTime.Now,
                    Status = true,
                    Discount = 0,
                    RoleID = adminRole.RoleID
                });
            }

            context.SaveChanges();
        }
    }
}