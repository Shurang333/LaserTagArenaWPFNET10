using LaserTagArenaWPFNET10.Controllers;
using LaserTagArenaWPFNET10.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace LaserTagArenaWPFNET10.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly AuthController _authController;

        public RegisterWindow()
        {
            InitializeComponent();
            _authController = App.ServiceProvider!.GetRequiredService<AuthController>();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Close();

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Сохраняем значения UI элементов ДО запуска задачи
            string lastName = txtLastName.Text.Trim();
            string firstName = txtFirstName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            // Блокируем кнопку, чтобы избежать повторных нажатий
            var btn = sender as System.Windows.Controls.Button;
            if (btn != null) btn.IsEnabled = false;

            try
            {
                // Запускаем задачу с уже сохранёнными значениями
                var result = await Task.Run(() => _authController.Register(
                    lastName,
                    firstName,
                    phone,
                    email,
                    password
                ));

                // Возвращаемся в UI поток для обновления интерфейса
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else
                {
                    txtStatus.Text = result.Message;
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = ExceptionTranslator.Translate(ex);
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }
    }
}