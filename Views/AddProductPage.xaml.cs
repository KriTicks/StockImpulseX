using System;
using System.Windows;
using System.Windows.Controls;
using StockImpulseX.Models;
using StockImpulseX.Services;

namespace StockImpulseX.Views
{
    public partial class AddProductPage : Page
    {
        private DatabaseService _dbService;

        public AddProductPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();

            // Установка дат по умолчанию
            dpManufactureDate.SelectedDate = DateTime.Today;
            dpExpiryDate.SelectedDate = DateTime.Today.AddDays(14);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Отменить добавление товара? Введённые данные будут потеряны.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ClearForm();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название товара!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpManufactureDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату производства!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpExpiryDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату годности!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpExpiryDate.SelectedDate <= dpManufactureDate.SelectedDate)
            {
                MessageBox.Show("Дата годности должна быть позже даты производства!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество (целое число больше 0)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создание товара
            var product = new Product
            {
                Name = txtName.Text.Trim(),
                Category = (cbCategory.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "",
                Location = (cbLocation.SelectedItem as ComboBoxItem)?.Content.ToString() == "📦 Склад" ? "Stock" : "Hall",
                ManufactureDate = dpManufactureDate.SelectedDate.Value,
                ExpiryDate = dpExpiryDate.SelectedDate.Value,
                Quantity = quantity,
                // Дополнительные поля (если добавишь в класс Product)
            };

            try
            {
                _dbService.AddProduct(product);
                MessageBox.Show("Товар успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            txtName.Clear();
            txtQuantity.Clear();
            txtBarcode?.Clear();
            txtSupplier?.Clear();
            txtPurchasePrice?.Clear();
            txtSellingPrice?.Clear();
            txtBatchNumber?.Clear();
            txtNote?.Clear();
            cbCategory.SelectedIndex = 0;
            cbLocation.SelectedIndex = 0;
            dpManufactureDate.SelectedDate = DateTime.Today;
            dpExpiryDate.SelectedDate = DateTime.Today.AddDays(14);
        }
    }
}