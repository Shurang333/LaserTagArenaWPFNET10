using System.Windows;

namespace LaserTagArenaWPFNET10.Views
{
    public partial class InputDialog : Window
    {
        public string InputText => txtInput.Text;  // <-- ДОБАВЬТЕ ЭТО

        public InputDialog(string prompt, string title = "Ввод")
        {
            InitializeComponent();
            lblPrompt.Text = prompt;
            Title = title;
            txtInput.Focus();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}