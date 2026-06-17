using System;
using LaserTagArenaWPFNET10.Models;

namespace LaserTagArenaWPFNET10.Helpers
{
    public static class StockHelper
    {
        public static void ApplyStockDelta(Equipment equipment, int delta)
        {
            if (equipment == null) return;

            equipment.StockQuantity = Math.Max(0, equipment.StockQuantity + delta);
            equipment.IsAvailable = equipment.StockQuantity > 0;
            equipment.UpdatedAt = DateTime.Now;
        }

        public static string FormatStockLine(string name, int added, int total) =>
            $"«{name}» +{added} шт. → {total} шт.";
    }
}