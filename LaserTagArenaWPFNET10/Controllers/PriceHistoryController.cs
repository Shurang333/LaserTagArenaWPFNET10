using System.Collections.Generic;
using System.Linq;
using LaserTagArenaWPFNET10.Data;
using LaserTagArenaWPFNET10.Models;
using Microsoft.EntityFrameworkCore;

namespace LaserTagArenaWPFNET10.Controllers
{
    public class PriceHistoryController
    {
        private readonly LaserTagDbContext _db;

        public PriceHistoryController(LaserTagDbContext db)
        {
            _db = db;
        }

        public List<PriceHistory> GetAll()
        {
            ApplicationSession.RequireAdmin();
            return _db.PriceHistories
                .Include(p => p.Equipment)
                .Include(p => p.ChangedByUser)
                .OrderByDescending(p => p.ChangeDate)
                .ToList();
        }
    }
}