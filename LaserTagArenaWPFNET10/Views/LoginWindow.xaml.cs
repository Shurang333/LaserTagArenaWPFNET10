using LaserTagArenaWPFNET10.Controllers;
using LaserTagArenaWPFNET10.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace LaserTagArenaWPFNET10.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthController _authController;

        public LoginWindow()
        {
            InitializeComponent();
            _authController = App.ServiceProvider!.GetRequiredService<AuthController>();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = txtLogin.Text.Trim();
                string password = txtPassword.Password;

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    txtStatus.Text = "Введите логин и пароль";
                    return;
                }

                var result = await System.Threading.Tasks.Task.Run(() => _authController.Login(login, password));

                if (result.Success)
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
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
        }
    }
}