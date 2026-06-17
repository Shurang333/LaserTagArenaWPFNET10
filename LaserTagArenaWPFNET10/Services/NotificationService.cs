using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagArenaWPFNET10.Controllers;
using LaserTagArenaWPFNET10.Data;
using LaserTagArenaWPFNET10.Helpers;
using LaserTagArenaWPFNET10.Models;

namespace LaserTagArenaWPFNET10.Services
{
    public class NotificationService
    {
        private readonly LaserTagDbContext _db;

        public NotificationService(LaserTagDbContext db)
        {
            _db = db;
        }

        public void Create(int userId, string message, string type)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(message)) return;

            try
            {
                _db.Notifications.Add(new Notification
                {
                    UserID = userId,
                    Message = message.Length > 500 ? message.Substring(0, 497) + "..." : message,
                    Type = type ?? NotificationTypes.System,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
                _db.SaveChanges();
            }
            catch (Exception ex) when (ExceptionTranslator.IsMissingTable(ex, "Notifications"))
            {
            }
        }

        public void NotifyCurrentUser(string message, string type) =>
            Create(ApplicationSession.CurrentUser?.UserID ?? 0, message, type);

        public void NotifyStaff(string message, string type)
        {
            try
            {
                var userIds = _db.Users
                    .Where(u => u.Status != false && (u.RoleID == UserRoles.Admin || u.RoleID == UserRoles.Manager))
                    .Select(u => u.UserID)
                    .ToList();

                foreach (int userId in userIds)
                    Create(userId, message, type);
            }
            catch (Exception ex) when (ExceptionTranslator.IsMissingTable(ex, "Notifications"))
            {
            }
        }

        public void NotifyAdmins(string message, string type)
        {
            try
            {
                var userIds = _db.Users
                    .Where(u => u.Status != false && u.RoleID == UserRoles.Admin)
                    .Select(u => u.UserID)
                    .ToList();

                foreach (int userId in userIds)
                    Create(userId, message, type);
            }
            catch (Exception ex) when (ExceptionTranslator.IsMissingTable(ex, "Notifications"))
            {
            }
        }

        public List<Notification> GetForUser(int userId, bool unreadOnly = false)
        {
            if (userId <= 0) return new List<Notification>();

            try
            {
                var query = _db.Notifications.Where(n => n.UserID == userId);
                if (unreadOnly)
                    query = query.Where(n => !n.IsRead);

                return query.OrderByDescending(n => n.CreatedAt).ToList();
            }
            catch (Exception ex) when (ExceptionTranslator.IsMissingTable(ex, "Notifications"))
            {
                return new List<Notification>();
            }
        }

        public int GetUnreadCount(int userId)
        {
            if (userId <= 0) return 0;

            try
            {
                return _db.Notifications.Count(n => n.UserID == userId && !n.IsRead);
            }
            catch (Exception ex) when (ExceptionTranslator.IsMissingTable(ex, "Notifications"))
            {
                return 0;
            }
        }

        public void MarkAllRead(int userId)
        {
            if (userId <= 0) return;

            try
            {
                var items = _db.Notifications.Where(n => n.UserID == userId && !n.IsRead).ToList();
                foreach (var item in items)
                    item.IsRead = true;
                _db.SaveChanges();
            }
            catch (Exception ex) when (ExceptionTranslator.IsMissingTable(ex, "Notifications"))
            {
            }
        }
    }
}
