using System;
using System.IO;
using System.Linq;
using LaserTagArenaWPFNET10.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace LaserTagArenaWPFNET10.Services
{
    public class PdfReceiptService
    {
        public string GenerateOrderReceipt(Order order, string? outputDirectory = null)
        {
            if (outputDirectory == null)
                outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts");

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            string fileName = $"Receipt_{order.OrderNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(outputDirectory, fileName);

            var document = new PdfDocument();
            document.Info.Title = $"Чек заказа {order.OrderNumber}";
            document.Info.Author = "LaserTag Arena";

            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            // ИСПРАВЛЕНО: Используем XFontStyleEx вместо XFontStyle
            var fontTitle = new XFont("Arial", 18, XFontStyleEx.Bold);
            var fontHeader = new XFont("Arial", 12, XFontStyleEx.Bold);
            var fontBody = new XFont("Arial", 11, XFontStyleEx.Regular);

            double y = 40;
            gfx.DrawString("LASERTAG ARENA", fontTitle, XBrushes.DarkGreen, new XRect(40, y, page.Width - 80, 30), XStringFormats.TopLeft);
            y += 35;
            gfx.DrawString("Чек о заказе", fontHeader, XBrushes.Black, 40, y);
            y += 25;
            gfx.DrawString($"Номер: {order.OrderNumber}", fontBody, XBrushes.Black, 40, y);
            y += 18;
            gfx.DrawString($"Дата: {order.OrderDateDisplay}", fontBody, XBrushes.Black, 40, y);
            y += 18;
            gfx.DrawString($"Клиент: {order.CustomerName}", fontBody, XBrushes.Black, 40, y);
            y += 18;
            gfx.DrawString($"Телефон: {order.CustomerPhone}", fontBody, XBrushes.Black, 40, y);
            y += 18;
            gfx.DrawString($"Статус: {order.StatusName}", fontBody, XBrushes.Black, 40, y);
            y += 25;

            gfx.DrawString("Позиции заказа:", fontHeader, XBrushes.Black, 40, y);
            y += 20;

            if (order.Items != null && order.Items.Any())
            {
                foreach (var item in order.Items)
                {
                    gfx.DrawString($"- {item.EquipmentName} ({item.EquipmentSKU}) x{item.Quantity} = {item.TotalPriceDisplay}",
                        fontBody, XBrushes.Black, 50, y);
                    y += 16;
                }
            }
            else
            {
                gfx.DrawString("(позиции не загружены)", fontBody, XBrushes.Gray, 50, y);
                y += 16;
            }

            y += 10;
            gfx.DrawString($"ИТОГО: {order.TotalAmountDisplay}", fontHeader, XBrushes.DarkGreen, 40, y);
            y += 30;
            gfx.DrawString("Спасибо за заказ!", fontBody, XBrushes.Black, 40, y);

            document.Save(filePath);
            return filePath;
        }
    }
}