using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using QRCoder;

namespace LaserTagArenaWPFNET10.Services
{
    public class QrCodeService
    {
        public BitmapImage GenerateQrImage(string content, int pixelsPerModule = 8)
        {
            using (var generator = new QRCodeGenerator())
            using (var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(data))
            using (Bitmap bitmap = qrCode.GetGraphic(pixelsPerModule))
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        public void SaveQrToFile(string content, string filePath)
        {
            using (var generator = new QRCodeGenerator())
            using (var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(data))
            using (Bitmap bitmap = qrCode.GetGraphic(8))
            {
                bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}