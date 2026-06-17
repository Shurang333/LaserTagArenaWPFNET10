using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LaserTagArenaWPFNET10.Controllers;
using LaserTagArenaWPFNET10.Helpers;
using LaserTagArenaWPFNET10.Models;
using LaserTagArenaWPFNET10.Services;
using LaserTagArenaWPFNET10.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LaserTagArenaWPFNET10.Views
{
    public partial class MainWindow : Window
    {
        private EquipmentController? _equipmentController;
        private OrderController? _orderController;
        private CartController? _cartController;
        private UserController? _userController;
        private ReferenceController? _referenceController;
        private ShipmentController? _shipmentController;
        private PriceHistoryController? _priceHistoryController;
        private NotificationController? _notificationController;
        private RecommendationController? _recommendationController;
        private PdfReceiptService? _pdfService;
        private QrCodeService? _qrService;
        private NotificationService? _notificationService;

        private System.Windows.Threading.DispatcherTimer? _timer;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Получаем сервисы через DI
                var services = App.ServiceProvider;
                if (services != null)
                {
                    _equipmentController = services.GetRequiredService<EquipmentController>();
                    _orderController = services.GetRequiredService<OrderController>();
                    _cartController = services.GetRequiredService<CartController>();
                    _userController = services.GetRequiredService<UserController>();
                    _referenceController = services.GetRequiredService<ReferenceController>();
                    _shipmentController = services.GetRequiredService<ShipmentController>();
                    _priceHistoryController = services.GetRequiredService<PriceHistoryController>();
                    _notificationController = services.GetRequiredService<NotificationController>();
                    _recommendationController = services.GetRequiredService<RecommendationController>();
                    _pdfService = services.GetRequiredService<PdfReceiptService>();
                    _qrService = services.GetRequiredService<QrCodeService>();
                    _notificationService = services.GetRequiredService<NotificationService>();
                }
                else
                {
                    MessageBox.Show("Ошибка инициализации сервисов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateUserInfo();
                LoadEquipment();
                LoadOrders();
                LoadShipments();
                LoadCategories();
                LoadManufacturers();
                LoadCart();
                LoadNotifications();

                // Таймер для обновления времени
                _timer = new System.Windows.Threading.DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += (s, args) => txtDateTime.Text = DateTime.Now.ToString("HH:mm:ss");
                _timer.Start();

                // Проверка прав администратора
                if (ApplicationSession.IsAdmin)
                {
                    tabAdmin.Visibility = Visibility.Visible;
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Ошибка загрузки: {ex.Message}";
            }
        }

        #region ====== ОБНОВЛЕНИЕ ИНФОРМАЦИИ ======

        private void UpdateUserInfo()
        {
            try
            {
                var user = ApplicationSession.CurrentUser;
                if (user != null)
                {
                    txtUserName.Text = user.FullName;
                    txtUserRole.Text = user.RoleName ?? "Пользователь";
                }
                else
                {
                    txtUserName.Text = "Гость";
                    txtUserRole.Text = "Не авторизован";
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Ошибка: {ex.Message}";
            }
        }

        #endregion

        #region ====== ОКНА (ОБРАБОТЧИКИ СОБЫТИЙ) ======

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnMaximize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Выйти из системы?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ApplicationSession.Clear();
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }

        private void BtnNotifications_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainTabControl.SelectedIndex = 8; // Вкладка уведомлений
            }
            catch { }
        }

        #endregion

        #region ====== ТОВАРЫ ======

        private void LoadEquipment()
        {
            try
            {
                if (_equipmentController == null) return;

                string sortBy = cmbEquipmentSort?.SelectedValue?.ToString() ?? "NameAsc";
                string search = txtSearch?.Text;
                string tagFilter = txtTagFilter?.Text;

                var list = _equipmentController.GetAll(sortBy, search, tagFilter);
                dgEquipment.ItemsSource = list;
                txtEquipmentCount.Text = $"ЗАГРУЖЕНО: {list?.Count ?? 0}";
                txtRecordCount.Text = $"ТОВАРОВ: {list?.Count ?? 0}";
                txtEquipmentStatus.Text = "> ГОТОВ";
            }
            catch (Exception ex)
            {
                txtEquipmentStatus.Text = $"ОШИБКА: {ex.Message}";
            }
        }

        private void BtnRefreshEquipment_Click(object sender, RoutedEventArgs e) => LoadEquipment();

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadEquipment();
        private void TxtTagFilter_TextChanged(object sender, TextChangedEventArgs e) => LoadEquipment();
        private void CmbEquipmentSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadEquipment();

        private void BtnAddEquipment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_equipmentController == null || _referenceController == null) return;

                var categories = _referenceController.GetCategories();
                var manufacturers = _referenceController.GetManufacturers();

                var dialog = new EquipmentEditWindow();
                dialog.Categories = categories;
                dialog.Manufacturers = manufacturers;
                dialog.cmbCategory.ItemsSource = categories;
                dialog.cmbManufacturer.ItemsSource = manufacturers;

                if (dialog.ShowDialog() == true)
                {
                    _equipmentController.Add(dialog.Equipment);
                    LoadEquipment();
                    txtEquipmentStatus.Text = $"> ТОВАР ДОБАВЛЕН: {dialog.Equipment.Name}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditEquipment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_equipmentController == null || _referenceController == null) return;

                if (dgEquipment.SelectedItem is Equipment selected)
                {
                    var categories = _referenceController.GetCategories();
                    var manufacturers = _referenceController.GetManufacturers();

                    var dialog = new EquipmentEditWindow(selected);
                    dialog.Categories = categories;
                    dialog.Manufacturers = manufacturers;
                    dialog.cmbCategory.ItemsSource = categories;
                    dialog.cmbManufacturer.ItemsSource = manufacturers;
                    dialog.cmbCategory.SelectedItem = categories.FirstOrDefault(c => c.CategoryID == selected.CategoryID);
                    dialog.cmbManufacturer.SelectedItem = manufacturers.FirstOrDefault(m => m.ManufacturerID == selected.ManufacturerID);

                    if (dialog.ShowDialog() == true)
                    {
                        _equipmentController.Update(dialog.Equipment);
                        LoadEquipment();
                        txtEquipmentStatus.Text = $"> ТОВАР ОБНОВЛЕН: {dialog.Equipment.Name}";
                    }
                }
                else
                {
                    MessageBox.Show("Выберите товар для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteEquipment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_equipmentController == null) return;

                if (dgEquipment.SelectedItem is Equipment selected)
                {
                    if (MessageBox.Show($"Удалить товар «{selected.Name}»?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _equipmentController.Delete(selected.EquipmentID);
                        LoadEquipment();
                        txtEquipmentStatus.Text = $"> ТОВАР УДАЛЕН: {selected.Name}";
                    }
                }
                else
                {
                    MessageBox.Show("Выберите товар для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_cartController == null) return;

                if (dgEquipment.SelectedItem is Equipment selected)
                {
                    var dialog = new InputDialog("Введите количество:", "Добавление в корзину");
                    if (dialog.ShowDialog() == true && int.TryParse(dialog.InputText, out int quantity) && quantity > 0)
                    {
                        int userId = ApplicationSession.CurrentUser?.UserID ?? 0;
                        if (userId > 0)
                        {
                            _cartController.AddToCart(userId, selected.EquipmentID, quantity);
                            LoadCart();
                            txtEquipmentStatus.Text = $"> В КОРЗИНУ: {selected.Name} × {quantity}";
                        }
                        else
                        {
                            MessageBox.Show("Необходимо войти в систему", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите товар", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region ====== ЗАКАЗЫ ======

        private void LoadOrders()
        {
            try
            {
                if (_orderController == null) return;

                string sortBy = cmbOrderSort?.SelectedValue?.ToString() ?? "DateDesc";
                int? statusId = null;
                if (!string.IsNullOrEmpty(cmbOrderStatus?.SelectedValue?.ToString()))
                {
                    if (int.TryParse(cmbOrderStatus.SelectedValue.ToString(), out int status))
                        statusId = status;
                }
                string search = txtOrderSearch?.Text;

                var list = _orderController.GetOrders(statusId, search, sortBy);
                dgOrders.ItemsSource = list;
                txtOrdersCount.Text = $"ЗАГРУЖЕНО: {list?.Count ?? 0}";
                txtOrdersStatus.Text = "> ГОТОВ";
            }
            catch (Exception ex)
            {
                txtOrdersStatus.Text = $"ОШИБКА: {ex.Message}";
            }
        }

        private void BtnRefreshOrders_Click(object sender, RoutedEventArgs e) => LoadOrders();
        private void TxtOrderSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadOrders();
        private void CmbOrderSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadOrders();
        private void CmbOrderStatus_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadOrders();

        private void BtnCreateOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_orderController == null) return;

                int userId = ApplicationSession.CurrentUser?.UserID ?? 0;
                if (userId <= 0)
                {
                    MessageBox.Show("Необходимо войти в систему", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var order = _orderController.CreateFromCart(userId, DateTime.Now, "Наличные");
                LoadOrders();
                LoadCart();
                txtOrdersStatus.Text = $"> ЗАКАЗ СОЗДАН: {order?.OrderNumber ?? "—"}";
                if (order != null)
                {
                    MessageBox.Show($"Заказ {order.OrderNumber} создан на сумму {order.TotalAmount:N0} ₽",
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_orderController == null) return;

                if (dgOrders.SelectedItem is Order selected)
                {
                    if (MessageBox.Show($"Удалить заказ {selected.OrderNumber}?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _orderController.Delete(selected.OrderID);
                        LoadOrders();
                        txtOrdersStatus.Text = $"> ЗАКАЗ УДАЛЕН: {selected.OrderNumber}";
                    }
                }
                else
                {
                    MessageBox.Show("Выберите заказ для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_orderController == null) return;

                if ((sender as Button)?.Tag is int orderId)
                {
                    var statuses = _orderController.GetOrderStatuses();
                    var dialog = new InputDialog("Введите новый статус (ID):", "Изменение статуса");
                    if (dialog.ShowDialog() == true && int.TryParse(dialog.InputText, out int statusId))
                    {
                        _orderController.UpdateStatus(orderId, statusId);
                        LoadOrders();
                        txtOrdersStatus.Text = $"> СТАТУС ИЗМЕНЕН";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPdfReceipt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_orderController == null) return;

                if (dgOrders.SelectedItem is Order selected)
                {
                    string path = _orderController.GeneratePdfReceipt(selected.OrderID);
                    MessageBox.Show($"Чек сохранен:\n{path}", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtOrdersStatus.Text = $"> PDF ЧЕК: {selected.OrderNumber}";
                }
                else
                {
                    MessageBox.Show("Выберите заказ", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnShowQr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgOrders.SelectedItem is Order selected)
                {
                    var qrWindow = new QrCodeWindow(selected.OrderNumber);
                    qrWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Выберите заказ", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region ====== ПОСТАВКИ ======

        private void LoadShipments()
        {
            try
            {
                if (_shipmentController == null) return;

                string sortBy = cmbShipmentSort?.SelectedValue?.ToString() ?? "DateDesc";
                int? statusId = null;
                if (!string.IsNullOrEmpty(cmbShipmentStatus?.SelectedValue?.ToString()))
                {
                    if (int.TryParse(cmbShipmentStatus.SelectedValue.ToString(), out int status))
                        statusId = status;
                }
                string search = txtShipmentSearch?.Text;

                var list = _shipmentController.GetAll(sortBy, statusId, search);
                dgShipments.ItemsSource = list;
                txtShipmentsCount.Text = $"ЗАГРУЖЕНО: {list?.Count ?? 0}";
                txtShipmentsStatus.Text = "> ГОТОВ";
            }
            catch (Exception ex)
            {
                txtShipmentsStatus.Text = $"ОШИБКА: {ex.Message}";
            }
        }

        private void BtnRefreshShipments_Click(object sender, RoutedEventArgs e) => LoadShipments();
        private void TxtShipmentSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadShipments();
        private void CmbShipmentSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadShipments();
        private void CmbShipmentStatus_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadShipments();

        private void BtnAddShipment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_shipmentController == null || _equipmentController == null) return;

                var suppliers = _shipmentController.GetSuppliers();
                var equipment = _equipmentController.GetAll();

                var dialog = new ShipmentCreateWindow(suppliers, equipment);
                if (dialog.ShowDialog() == true && dialog.CreatedShipment != null)
                {
                    _shipmentController.Add(dialog.CreatedShipment);
                    LoadShipments();
                    txtShipmentsStatus.Text = $"> ПОСТАВКА СОЗДАНА: {dialog.CreatedShipment.ShipmentNumber}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCompleteShipment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_shipmentController == null) return;

                if (dgShipments.SelectedItem is Shipment selected)
                {
                    if (MessageBox.Show($"Завершить поставку {selected.ShipmentNumber}?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        var result = _shipmentController.Complete(selected.ShipmentID);
                        if (result.Success)
                        {
                            LoadShipments();
                            txtShipmentsStatus.Text = $"> ПОСТАВКА ЗАВЕРШЕНА: {result.ShipmentNumber} ({result.ItemsUpdated} товаров)";
                            LoadEquipment();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите поставку", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region ====== СПРАВОЧНИКИ ======

        private void LoadCategories()
        {
            try
            {
                if (_referenceController == null) return;
                var list = _referenceController.GetCategories();
                dgCategories.ItemsSource = list;
            }
            catch (Exception ex)
            {
                // Игнорируем
            }
        }

        private void LoadManufacturers()
        {
            try
            {
                if (_referenceController == null) return;
                var list = _referenceController.GetManufacturers();
                dgManufacturers.ItemsSource = list;
            }
            catch (Exception ex)
            {
                // Игнорируем
            }
        }

        private void BtnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_referenceController == null) return;

                var dialog = new InputDialog("Введите название категории:", "Новая категория");
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
                {
                    _referenceController.AddCategory(dialog.InputText);
                    LoadCategories();
                    txtStatus.Text = $"> КАТЕГОРИЯ ДОБАВЛЕНА: {dialog.InputText}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_referenceController == null) return;

                if (dgCategories.SelectedItem is Category selected)
                {
                    if (MessageBox.Show($"Удалить категорию «{selected.Name}»?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _referenceController.DeleteCategory(selected.CategoryID);
                        LoadCategories();
                        txtStatus.Text = $"> КАТЕГОРИЯ УДАЛЕНА: {selected.Name}";
                    }
                }
                else
                {
                    MessageBox.Show("Выберите категорию", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddManufacturer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_referenceController == null) return;

                var dialog = new InputDialog("Введите название производителя:", "Новый производитель");
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
                {
                    _referenceController.AddManufacturer(dialog.InputText);
                    LoadManufacturers();
                    txtStatus.Text = $"> ПРОИЗВОДИТЕЛЬ ДОБАВЛЕН: {dialog.InputText}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteManufacturer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_referenceController == null) return;

                if (dgManufacturers.SelectedItem is Manufacturer selected)
                {
                    if (MessageBox.Show($"Удалить производителя «{selected.Name}»?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _referenceController.DeleteManufacturer(selected.ManufacturerID);
                        LoadManufacturers();
                        txtStatus.Text = $"> ПРОИЗВОДИТЕЛЬ УДАЛЕН: {selected.Name}";
                    }
                }
                else
                {
                    MessageBox.Show("Выберите производителя", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region ====== ИСТОРИЯ ЦЕН ======

        private void BtnRefreshPriceHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_priceHistoryController == null) return;

                var list = _priceHistoryController.GetAll();
                dgPriceHistory.ItemsSource = list;
                txtHistoryCount.Text = $"ЗАПИСЕЙ: {list?.Count ?? 0}";
                txtHistoryStatus.Text = "> ГОТОВ";
            }
            catch (Exception ex)
            {
                txtHistoryStatus.Text = $"ОШИБКА: {ex.Message}";
            }
        }

        #endregion

        #region ====== КОРЗИНА ======

        private void LoadCart()
        {
            try
            {
                if (_cartController == null) return;

                int userId = ApplicationSession.CurrentUser?.UserID ?? 0;
                if (userId > 0)
                {
                    var list = _cartController.GetCart(userId);
                    dgCart.ItemsSource = list;
                }
                else
                {
                    dgCart.ItemsSource = new List<CartItem>();
                }
            }
            catch (Exception ex)
            {
                // Игнорируем
            }
        }

        private void BtnRefreshCart_Click(object sender, RoutedEventArgs e) => LoadCart();

        private void BtnRemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_cartController == null) return;

                if (dgCart.SelectedItem is CartItem selected)
                {
                    _cartController.RemoveFromCart(selected.CartItemID);
                    LoadCart();
                    txtStatus.Text = $"> ИЗ КОРЗИНЫ УДАЛЕНО: {selected.EquipmentName}";
                }
                else
                {
                    MessageBox.Show("Выберите позицию", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCheckoutCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_orderController == null) return;

                int userId = ApplicationSession.CurrentUser?.UserID ?? 0;
                if (userId <= 0)
                {
                    MessageBox.Show("Необходимо войти в систему", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dialog = new InputDialog("Введите дату игры (дд.мм.гггг) или оставьте пустым:", "Оформление заказа");
                DateTime? gameDate = null;
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
                {
                    if (DateTime.TryParse(dialog.InputText, out DateTime date))
                        gameDate = date;
                }

                var order = _orderController.CreateFromCart(userId, gameDate, "Наличные");
                LoadOrders();
                LoadCart();
                txtStatus.Text = $"> ЗАКАЗ ОФОРМЛЕН: {order?.OrderNumber ?? "—"}";
                if (order != null)
                {
                    MessageBox.Show($"Заказ {order.OrderNumber} оформлен на сумму {order.TotalAmount:N0} ₽",
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region ====== ИИ / РЕКОМЕНДАЦИИ ======

        private async void BtnAiAdvice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_recommendationController == null) return;

                string query = txtAiQuery.Text;
                if (string.IsNullOrWhiteSpace(query))
                {
                    MessageBox.Show("Введите запрос", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                txtAiResponse.Text = "Загрузка...";
                var advice = await _recommendationController.GetAiAdviceAsync(query);
                txtAiResponse.Text = advice;

                var recommendations = _recommendationController.GetForQuery(query);
                lstRecommendations.ItemsSource = recommendations;
            }
            catch (Exception ex)
            {
                txtAiResponse.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void BtnRecommendByTags_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_recommendationController == null) return;

                string tags = txtAiQuery.Text;
                if (string.IsNullOrWhiteSpace(tags))
                {
                    MessageBox.Show("Введите теги", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var recommendations = _recommendationController.GetByTags(tags);
                lstRecommendations.ItemsSource = recommendations;
                txtAiResponse.Text = $"Найдено {recommendations?.Count ?? 0} рекомендаций по тегам: {tags}";
            }
            catch (Exception ex)
            {
                txtAiResponse.Text = $"Ошибка: {ex.Message}";
            }
        }

        #endregion

        #region ====== АДМИНИСТРИРОВАНИЕ ======

        private void LoadUsers()
        {
            try
            {
                if (_userController == null) return;

                var list = _userController.GetAll();
                dgUsers.ItemsSource = list;
                txtAdminHint.Text = $"> ПОЛЬЗОВАТЕЛЕЙ: {list?.Count ?? 0}";
            }
            catch (Exception ex)
            {
                txtAdminHint.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void BtnRefreshUsers_Click(object sender, RoutedEventArgs e) => LoadUsers();

        private void BtnActivateUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_userController == null) return;

                if (dgUsers.SelectedItem is User selected)
                {
                    _userController.SetStatus(selected.UserID, true);
                    LoadUsers();
                    txtAdminHint.Text = $"> АКТИВИРОВАН: {selected.FullName}";
                }
                else
                {
                    MessageBox.Show("Выберите пользователя", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBlockUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_userController == null) return;

                if (dgUsers.SelectedItem is User selected)
                {
                    if (MessageBox.Show($"Заблокировать пользователя {selected.FullName}?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _userController.SetStatus(selected.UserID, false);
                        LoadUsers();
                        txtAdminHint.Text = $"> ЗАБЛОКИРОВАН: {selected.FullName}";
                    }
                }
                else
                {
                    MessageBox.Show("Выберите пользователя", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMakeAdmin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_userController == null) return;

                if (dgUsers.SelectedItem is User selected)
                {
                    _userController.SetRole(selected.UserID, UserRoles.Admin);
                    LoadUsers();
                    txtAdminHint.Text = $"> НАЗНАЧЕН АДМИНОМ: {selected.FullName}";
                }
                else
                {
                    MessageBox.Show("Выберите пользователя", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMakeManager_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_userController == null) return;

                if (dgUsers.SelectedItem is User selected)
                {
                    _userController.SetRole(selected.UserID, UserRoles.Manager);
                    LoadUsers();
                    txtAdminHint.Text = $"> НАЗНАЧЕН МЕНЕДЖЕРОМ: {selected.FullName}";
                }
                else
                {
                    MessageBox.Show("Выберите пользователя", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region ====== УВЕДОМЛЕНИЯ ======

        private void LoadNotifications()
        {
            try
            {
                if (_notificationController == null) return;

                var list = _notificationController.GetAll();
                lstNotifications.ItemsSource = list;
                int unread = _notificationController.GetUnreadCount();
                txtNotificationBadge.Text = unread > 0 ? unread.ToString() : "";
            }
            catch
            {
                // Игнорируем
            }
        }

        private void BtnRefreshNotifications_Click(object sender, RoutedEventArgs e) => LoadNotifications();

        private void BtnMarkNotificationsRead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_notificationController == null) return;

                _notificationController.MarkAllRead();
                LoadNotifications();
                txtStatus.Text = "> УВЕДОМЛЕНИЯ ПРОЧИТАНЫ";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionTranslator.Translate(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region ====== ПЕРЕКЛЮЧЕНИЕ ВКЛАДОК ======

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var tab = mainTabControl.SelectedItem as TabItem;
                if (tab == null) return;

                string header = tab.Header?.ToString() ?? "";

                if (header.Contains("ТОВАРЫ"))
                    LoadEquipment();
                else if (header.Contains("ЗАКАЗЫ"))
                    LoadOrders();
                else if (header.Contains("ПОСТАВКИ"))
                    LoadShipments();
                else if (header.Contains("СПРАВОЧНИКИ"))
                {
                    LoadCategories();
                    LoadManufacturers();
                }
                else if (header.Contains("ИСТОРИЯ ЦЕН"))
                    BtnRefreshPriceHistory_Click(null, null);
                else if (header.Contains("КОРЗИНА"))
                    LoadCart();
                else if (header.Contains("УВЕДОМЛЕНИЯ"))
                    LoadNotifications();
                else if (header.Contains("АДМИН") && ApplicationSession.IsAdmin)
                    LoadUsers();
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Ошибка: {ex.Message}";
            }
        }

        #endregion
    }
}