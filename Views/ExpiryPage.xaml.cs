using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using StockImpulseX.Services;
using StockImpulseX.Models;
using Microsoft.Win32;
using System.Text;
using System.IO;
using System.Timers;

namespace StockImpulseX.Views
{
    public partial class ExpiryPage : Page
    {
        private DatabaseService _dbService;
        private ObservableCollection<Product> _allItems;
        private ObservableCollection<Product> _filteredItems;
        private int _pageSize = 15;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private System.Timers.Timer _refreshTimer;

        public ExpiryPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            LoadData();

            _refreshTimer = new System.Timers.Timer(300000);
            _refreshTimer.Elapsed += RefreshTimer_Elapsed;
            _refreshTimer.Start();
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => LoadData());
        }

        private void LoadData()
        {
            try
            {
                var products = _dbService.GetAllProducts();
                _allItems = new ObservableCollection<Product>(products);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _allItems = new ObservableCollection<Product>();
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            if (_allItems == null) return;

            var filtered = _allItems.AsEnumerable();

            string filter = (cbFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (filter == "🔴 Красные (1-3 дня)")
                filtered = filtered.Where(x => x.DaysUntilExpiry >= 1 && x.DaysUntilExpiry <= 3);
            else if (filter == "🟠 Оранжевые (4-7 дней)")
                filtered = filtered.Where(x => x.DaysUntilExpiry >= 4 && x.DaysUntilExpiry <= 7);
            else if (filter == "⚫ Просроченные")
                filtered = filtered.Where(x => x.DaysUntilExpiry <= 0);

            string query = txtSearch?.Text?.ToLower() ?? "";
            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(query)) ||
                    (x.Category != null && x.Category.ToLower().Contains(query)) ||
                    x.Id.ToString().Contains(query)
                );
            }

            _filteredItems = new ObservableCollection<Product>(filtered);
            _currentPage = 1;
            ApplyPagination();
        }

        private void ApplyPagination()
        {
            if (dgProducts == null || lblStats == null) return;

            if (_filteredItems == null || _filteredItems.Count == 0)
            {
                _totalPages = 0;
                dgProducts.ItemsSource = null;
                lblStats.Text = "Записей: 0";
                pnlPageNumbers?.Children.Clear();
                if (btnPrev != null) btnPrev.IsEnabled = false;
                if (btnNext != null) btnNext.IsEnabled = false;
                return;
            }

            _totalPages = (int)Math.Ceiling((double)_filteredItems.Count / _pageSize);
            if (_currentPage > _totalPages) _currentPage = _totalPages;
            if (_currentPage < 1) _currentPage = 1;

            var skip = (_currentPage - 1) * _pageSize;
            var items = _filteredItems.Skip(skip).Take(_pageSize).ToList();

            dgProducts.ItemsSource = items;

            int startRecord = skip + 1;
            int endRecord = Math.Min(skip + _pageSize, _filteredItems.Count);
            lblStats.Text = $"Записей {startRecord}-{endRecord} из {_filteredItems.Count}";

            RenderPageNumbers();

            if (btnPrev != null) btnPrev.IsEnabled = _currentPage > 1;
            if (btnNext != null) btnNext.IsEnabled = _currentPage < _totalPages;
        }

        private void RenderPageNumbers()
        {
            if (pnlPageNumbers == null) return;
            pnlPageNumbers.Children.Clear();
            if (_totalPages <= 1) return;

            int startPage = Math.Max(1, _currentPage - 2);
            int endPage = Math.Min(_totalPages, _currentPage + 2);

            for (int i = startPage; i <= endPage; i++)
            {
                Button btn = new Button();
                btn.Content = i.ToString();
                btn.Width = 35;
                btn.Height = 30;
                btn.Margin = new Thickness(2, 0, 2, 0);
                btn.FontSize = 13;
                btn.Cursor = Cursors.Hand;
                btn.Background = Brushes.Transparent;
                btn.Foreground = Brushes.White;
                btn.BorderBrush = new SolidColorBrush(Color.FromRgb(85, 85, 85));
                btn.BorderThickness = new Thickness(1);

                if (i == _currentPage)
                {
                    btn.Background = new SolidColorBrush(Color.FromRgb(200, 70, 70));
                    btn.Foreground = Brushes.White;
                    btn.FontWeight = FontWeights.Bold;
                }

                int pageIndex = i;
                btn.Click += (s, e) => { _currentPage = pageIndex; ApplyPagination(); };

                pnlPageNumbers.Children.Add(btn);
            }
        }

        private void CbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1) { _currentPage--; ApplyPagination(); }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages) { _currentPage++; ApplyPagination(); }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV файлы (*.csv)|*.csv";
                saveDialog.FileName = $"Товары_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (saveDialog.ShowDialog() == true)
                {
                    var items = _filteredItems ?? _allItems;
                    if (items == null || items.Count == 0)
                    {
                        MessageBox.Show("Нет данных для экспорта", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("ID;Название;Категория;Местоположение;Количество;Дата производства;Годен до;Дней осталось;Статус");

                    foreach (var p in items)
                    {
                        string status = p.DaysUntilExpiry < 0 ? "Просрочен" :
                                       p.DaysUntilExpiry <= 3 ? "Критично" :
                                       p.DaysUntilExpiry <= 7 ? "Внимание" : "Норма";

                        string location = p.Location == "Stock" ? "Склад" : "Торговый зал";

                        sb.AppendLine($"{p.Id};{EscapeCsv(p.Name)};{EscapeCsv(p.Category)};{location};{p.Quantity};{p.ManufactureDate:dd.MM.yyyy};{p.ExpiryDate:dd.MM.yyyy};{p.DaysUntilExpiry};{status}");
                    }

                    File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Экспорт завершён!\n{saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            }
        }

        private string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Contains(";") || text.Contains("\""))
            {
                return "\"" + text.Replace("\"", "\"\"") + "\"";
            }
            return text;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Product product)
            {
                var result = MessageBox.Show($"Удалить товар \"{product.Name}\"?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _dbService.DeleteProduct(product.Id);
                    LoadData();
                }
            }
        }
    }
}