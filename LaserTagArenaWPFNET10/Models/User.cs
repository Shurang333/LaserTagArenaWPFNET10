using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaserTagArenaWPFNET10.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; }
        public bool? Status { get; set; }
        public int Discount { get; set; }
        public int RoleID { get; set; }

        [ForeignKey(nameof(RoleID))]
        public Role? Role { get; set; }

        [NotMapped]
        public string? RoleName => Role?.RoleName;

        [NotMapped]
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        [NotMapped]
        public string StatusText => Status != false ? "Активен" : "Заблокирован";
    }

    public static class UserRoles
    {
        public const int Admin = 1;
        public const int Client = 2;
        public const int Manager = 3;
        public const int SiteAdmin = 4;
    }
}