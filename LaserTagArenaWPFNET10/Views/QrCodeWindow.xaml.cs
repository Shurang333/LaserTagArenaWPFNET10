using System.Windows;
using System.Windows.Media.Imaging;
using LaserTagArenaWPFNET10.Services;

namespace LaserTagArenaWPFNET10.Views
{
    public partial class QrCodeWindow : Window
    {
        private readonly QrCodeService _qrService = new QrCodeService();

        public QrCodeWindow(string content)  // <-- ТОЛЬКО 1 ПАРАМЕТР
        {
            InitializeComponent();
            GenerateQr(content);
        }

        private void GenerateQr(string content)
        {
            try
            {
                BitmapImage qrImage = _qrService.GenerateQrImage(content);
                imgQr.Source = qrImage;
            }
            catch
            {
                MessageBox.Show("Ошибка генерации QR-кода", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}