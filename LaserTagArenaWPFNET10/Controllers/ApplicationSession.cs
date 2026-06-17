using LaserTagArenaWPFNET10.Helpers;
using LaserTagArenaWPFNET10.Models;
using System.ComponentModel.DataAnnotations;
using ValidationException = LaserTagArenaWPFNET10.Helpers.ValidationException;

namespace LaserTagArenaWPFNET10.Controllers
{
    public static class ApplicationSession
    {
        public static User? CurrentUser { get; set; }

        public static bool IsAdmin => CurrentUser?.RoleID == UserRoles.Admin;

        public static bool IsManager => CurrentUser?.RoleID == UserRoles.Manager;

        public static void RequireAdmin()
        {
            if (!IsAdmin)
                throw new ValidationException("Доступ только для администратора.");
        }

        public static void Clear()
        {
            CurrentUser = null;
        }
    }
}