using System;
using System.Collections.Generic;
using System.Windows;
using LaserTagArenaWPFNET10.Models;

namespace LaserTagArenaWPFNET10.Views
{
    public partial class EquipmentEditWindow : Window
    {
        public Equipment Equipment { get; private set; }
        public List<Category> Categories { get; set; }
        public List<Manufacturer> Manufacturers { get; set; }

        public EquipmentEditWindow(Equipment equipment = null)
        {
            InitializeComponent();

            if (equipment != null)
            {
                Equipment = equipment;
                LoadEquipment();
            }
            else
            {
                Equipment = new Equipment();
            }
        }

        private void LoadEquipment()
        {
            txtSKU.Text = Equipment.SKU;
            txtName.Text = Equipment.Name;
            txtDescription.Text = Equipment.Description;
            txtPrice.Text = Equipment.CurrentPrice.ToString("F2");
            txtOldPrice.Text = Equipment.OldPrice?.ToString("F2");
            txtDiscount.Text = Equipment.DiscountPercent.ToString();
            txtStock.Text = Equipment.StockQuantity.ToString();
            txtTags.Text = Equipment.Tags;
            chkAvailable.IsChecked = Equipment.IsAvailable;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Equipment.SKU = txtSKU.Text.Trim();
                Equipment.Name = txtName.Text.Trim();
                Equipment.Description = txtDescription.Text.Trim();
                Equipment.CurrentPrice = decimal.Parse(txtPrice.Text.Replace(",", "."));
                Equipment.OldPrice = string.IsNullOrWhiteSpace(txtOldPrice.Text) ? null : decimal.Parse(txtOldPrice.Text.Replace(",", "."));
                Equipment.DiscountPercent = int.Parse(txtDiscount.Text);
                Equipment.StockQuantity = int.Parse(txtStock.Text);
                Equipment.Tags = txtTags.Text.Trim();
                Equipment.IsAvailable = chkAvailable.IsChecked ?? true;

                if (cmbCategory.SelectedItem is Category cat)
                    Equipment.CategoryID = cat.CategoryID;

                if (cmbManufacturer.SelectedItem is Manufacturer man)
                    Equipment.ManufacturerID = man.ManufacturerID;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}