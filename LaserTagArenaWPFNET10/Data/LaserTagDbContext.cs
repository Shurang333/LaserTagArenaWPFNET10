using LaserTagArenaWPFNET10.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection.Emit;
using System.Windows.Controls;

namespace LaserTagArenaWPFNET10.Data
{
    public class LaserTagDbContext : DbContext
    {
        public LaserTagDbContext(DbContextOptions<LaserTagDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentStatus> ShipmentStatuses { get; set; }
        public DbSet<ShipmentItem> ShipmentItems { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureDecimals(modelBuilder);
            ConfigureRelationships(modelBuilder);

            // Настройка имен таблиц (если отличаются от моделей)
            modelBuilder.Entity<OrderStatus>().ToTable("OrderStatuses");
            modelBuilder.Entity<ShipmentStatus>().ToTable("ShipmentStatuses");
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // User -> Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleID)
                .OnDelete(DeleteBehavior.Restrict);

            // Equipment -> Unit
            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.Unit)
                .WithMany()
                .HasForeignKey(e => e.UnitID)
                .OnDelete(DeleteBehavior.Restrict);

            // Equipment -> Supplier (опционально)
            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierID)
                .OnDelete(DeleteBehavior.SetNull);

            // Equipment -> Manufacturer (опционально)
            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.Manufacturer)
                .WithMany()
                .HasForeignKey(e => e.ManufacturerID)
                .OnDelete(DeleteBehavior.SetNull);

            // Equipment -> Category (опционально)
            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.SetNull);

            // Order -> Status
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Status)
                .WithMany()
                .HasForeignKey(o => o.StatusID)
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> User
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem -> Order
            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderID)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem -> Equipment
            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Equipment)
                .WithMany()
                .HasForeignKey(i => i.EquipmentID)
                .OnDelete(DeleteBehavior.Restrict);

            // Shipment -> Status
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Status)
                .WithMany()
                .HasForeignKey(s => s.StatusID)
                .OnDelete(DeleteBehavior.Restrict);

            // Shipment -> Supplier
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Supplier)
                .WithMany()
                .HasForeignKey(s => s.SupplierID)
                .OnDelete(DeleteBehavior.Restrict);

            // ShipmentItem -> Shipment
            modelBuilder.Entity<ShipmentItem>()
                .HasOne(i => i.Shipment)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.ShipmentID)
                .OnDelete(DeleteBehavior.Cascade);

            // ShipmentItem -> Equipment
            modelBuilder.Entity<ShipmentItem>()
                .HasOne(i => i.Equipment)
                .WithMany()
                .HasForeignKey(i => i.EquipmentID)
                .OnDelete(DeleteBehavior.Restrict);

            // PriceHistory -> Equipment
            modelBuilder.Entity<PriceHistory>()
                .HasOne(p => p.Equipment)
                .WithMany()
                .HasForeignKey(p => p.EquipmentID)
                .OnDelete(DeleteBehavior.Restrict);

            // PriceHistory -> ChangedByUser (опционально)
            modelBuilder.Entity<PriceHistory>()
                .HasOne(p => p.ChangedByUser)
                .WithMany()
                .HasForeignKey(p => p.ChangedByUserID)
                .OnDelete(DeleteBehavior.SetNull);

            // Notification -> User
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem -> User
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem -> Equipment
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Equipment)
                .WithMany()
                .HasForeignKey(c => c.EquipmentID)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureDecimals(ModelBuilder modelBuilder)
        {
            // Настройка decimal для SQL Server
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetPrecision(18);
                        property.SetScale(2);
                    }
                }
            }
        }
    }
}