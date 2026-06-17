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
    public class UserController
    {
        private readonly LaserTagDbContext _db;
        private readonly NotificationService _notifications;

        public UserController(LaserTagDbContext db, NotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public List<User> GetAll()
        {
            ApplicationSession.RequireAdmin();
            try
            {
                return _db.Users
                    .Include(u => u.Role)
                    .OrderBy(u => u.RoleID)
                    .ThenBy(u => u.LastName)
                    .ToList();
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public void SetStatus(int userId, bool active)
        {
            ApplicationSession.RequireAdmin();
            if (userId <= 0)
                throw new ValidationException("Некорректный пользователь.");

            if (ApplicationSession.CurrentUser?.UserID == userId && !active)
                throw new ValidationException("Нельзя заблокировать свою учётную запись.");

            try
            {
                var user = _db.Users.Find(userId);
                if (user == null)
                    throw new ValidationException("Пользователь не найден.");

                user.Status = active;
                _db.SaveChanges();

                _notifications.NotifyAdmins(
                    active
                        ? $"Активирован пользователь {user.LastName} {user.FirstName} ({user.Email})"
                        : $"Заблокирован пользователь {user.LastName} {user.FirstName} ({user.Email})",
                    active ? NotificationTypes.UserActivate : NotificationTypes.UserBlock);
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }

        public void SetRole(int userId, int roleId)
        {
            ApplicationSession.RequireAdmin();
            if (userId <= 0)
                throw new ValidationException("Некорректный пользователь.");

            if (roleId != UserRoles.Admin && roleId != UserRoles.Manager)
                throw new ValidationException("Можно назначить только роль «Администратор» или «Менеджер».");

            if (ApplicationSession.CurrentUser?.UserID == userId)
                throw new ValidationException("Нельзя изменить свою роль.");

            try
            {
                var user = _db.Users.Find(userId);
                if (user == null)
                    throw new ValidationException("Пользователь не найден.");

                if (!_db.Roles.Any(r => r.RoleID == roleId))
                    throw new ValidationException("Роль не найдена в базе.");

                string roleName = _db.Roles.Find(roleId)?.RoleName ?? "—";
                user.RoleID = roleId;
                _db.SaveChanges();

                _notifications.NotifyAdmins(
                    $"Роль изменена: {user.LastName} {user.FirstName} → {roleName}",
                    NotificationTypes.UserRoleChange);
            }
            catch (ValidationException) { throw; }
            catch (Exception ex) { throw new ValidationException(ExceptionTranslator.Translate(ex)); }
        }
    }
}