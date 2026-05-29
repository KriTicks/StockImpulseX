using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using StockImpulseX.Views;

namespace StockImpulseX
{
    public partial class MainWindow : Window
    {
        private bool isMenuOpen = false;
        private bool isAdminMenuOpen = false;

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new ExpiryPage());
        }

        private void BurgerButton_Click(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation();
            animation.Duration = TimeSpan.FromSeconds(0.3);
            animation.EasingFunction = new QuadraticEase();

            if (!isMenuOpen)
            {
                animation.To = 0;
                Overlay.Visibility = Visibility.Visible;
                var opacityAnim = new DoubleAnimation(0, 0.5, TimeSpan.FromSeconds(0.3));
                Overlay.BeginAnimation(OpacityProperty, opacityAnim);
            }
            else
            {
                animation.To = -220;
                var opacityAnim = new DoubleAnimation(0.5, 0, TimeSpan.FromSeconds(0.3));
                Overlay.BeginAnimation(OpacityProperty, opacityAnim);
                var timer = new System.Timers.Timer(300);
                timer.Elapsed += (s, args) => Dispatcher.Invoke(() => Overlay.Visibility = Visibility.Collapsed);
                timer.AutoReset = false;
                timer.Start();
            }

            MenuTransform.BeginAnimation(TranslateTransform.XProperty, animation);
            isMenuOpen = !isMenuOpen;
        }

        private void CloseMenu_Click(object sender, MouseButtonEventArgs e)
        {
            if (isMenuOpen) BurgerButton_Click(null, null);
        }

        private void Profile_Click(object sender, MouseButtonEventArgs e)
        {
            ProfilePopup.IsOpen = !ProfilePopup.IsOpen;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Выйти из программы?", "Выход", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void BtnAdminHeader_Click(object sender, RoutedEventArgs e)
        {
            isAdminMenuOpen = !isAdminMenuOpen;
            AdminSubMenu.Visibility = isAdminMenuOpen ? Visibility.Visible : Visibility.Collapsed;
            BtnAdminHeader.Content = isAdminMenuOpen ? "▲ Администрирование" : "▼ Администрирование";
        }

        private void ShowComingSoonMessage()
        {
            MessageBox.Show("📢 Страница будет доступна в следующем обновлении!\n\nСледите за обновлениями на GitHub.",
                "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowSettingsMessage()
        {
            MessageBox.Show("⚙️ Настройки будут доступны в следующем обновлении!\n\nСледите за обновлениями на GitHub.",
                "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowComingSoonMessage();
            PageTitle.Text = "Дашборд";
            if (isMenuOpen) BurgerButton_Click(null, null);
        }

        private void BtnStock_Click(object sender, RoutedEventArgs e)
        {
            ShowComingSoonMessage();
            PageTitle.Text = "Склад";
            if (isMenuOpen) BurgerButton_Click(null, null);
        }

        private void BtnHall_Click(object sender, RoutedEventArgs e)
        {
            ShowComingSoonMessage();
            PageTitle.Text = "Торговый зал";
            if (isMenuOpen) BurgerButton_Click(null, null);
        }

        private void BtnExpiry_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ExpiryPage());
            PageTitle.Text = "Контроль сроков";
            if (isMenuOpen) BurgerButton_Click(null, null);
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AddProductPage());
            PageTitle.Text = "Добавление товара";
            if (isMenuOpen) BurgerButton_Click(null, null);
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsMessage();
            ProfilePopup.IsOpen = false;
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            ProfilePopup.IsOpen = false;
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }
    }
}