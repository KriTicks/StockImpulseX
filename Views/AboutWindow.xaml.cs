using System.Diagnostics;
using System.Windows;

namespace StockImpulseX
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void GitHub_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/KriTicks") { UseShellExecute = true });
        }

        private void Telegram_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://t.me/KriTicks") { UseShellExecute = true });
        }
    }
}