using LaserTagArenaWPFNET10.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;  // <-- ДОБАВЬТЕ ЭТО

namespace LaserTagArenaWPFNET10.Views
{
    public partial class ShipmentCreateWindow : Window
    {
        public List<Supplier> Suppliers { get; set; }
        public List<Equipment> EquipmentList { get; set; }
        public List<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
        public Shipment CreatedShipment { get; set; }  // <-- ДОБАВЬТЕ ЭТО

        public ShipmentCreateWindow(List<Supplier> suppliers, List<Equipment> equipment)
        {
            InitializeComponent();
            Suppliers = suppliers;
            EquipmentList = equipment;
            LoadData();
        }

        private void LoadData()
        {
            cmbSupplier.ItemsSource = Suppliers;
            cmbEquipment.ItemsSource = EquipmentList;
            dgItems.ItemsSource = Items;
        }

        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEquipment.SelectedItem is Equipment eq &&
                int.TryParse(txtQuantity.Text, out int qty) && qty > 0)
            {
                Items.Add(new ShipmentItem
                {
                    EquipmentID = eq.EquipmentID,
                    Equipment = eq,
                    Quantity = qty,
                    PricePerUnit = eq.CurrentPrice
                });
                dgItems.Items.Refresh();
                txtQuantity.Text = "";
            }
        }

        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is ShipmentItem item)
            {
                Items.Remove(item);
                dgItems.Items.Refresh();
            }
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSupplier.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Items.Any())
            {
                MessageBox.Show("Добавьте хотя бы один товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CreatedShipment = new Shipment
            {
                SupplierID = ((Supplier)cmbSupplier.SelectedItem).SupplierID,
                ShipmentDate = DateTime.Now,
                Items = Items.ToList(),
                TotalCost = Items.Sum(i => i.Quantity * i.PricePerUnit)
            };

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