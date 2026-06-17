using LaserTagArenaWPFNET10.Services;
using LaserTagArenaWPFNET10.Models;
using System.Collections.Generic;

namespace LaserTagArenaWPFNET10.Controllers
{
    public class NotificationController
    {
        private readonly NotificationService _service;

        public NotificationController(NotificationService service)
        {
            _service = service;
        }

        public List<Notification> GetAll(bool unreadOnly = false)
        {
            int userId = ApplicationSession.CurrentUser?.UserID ?? 0;
            if (userId == 0) return new List<Notification>();
            return _service.GetForUser(userId, unreadOnly);
        }

        public int GetUnreadCount()
        {
            int userId = ApplicationSession.CurrentUser?.UserID ?? 0;
            if (userId == 0) return 0;
            return _service.GetUnreadCount(userId);
        }

        public void MarkAllRead()
        {
            int userId = ApplicationSession.CurrentUser?.UserID ?? 0;
            if (userId == 0) return;
            _service.MarkAllRead(userId);
        }
    }
}