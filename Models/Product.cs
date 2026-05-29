using System;

namespace StockImpulseX.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public DateTime ManufactureDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Location { get; set; }
        public int Quantity { get; set; }

        public int DaysUntilExpiry => (ExpiryDate - DateTime.Today).Days;

        public string ExpiryStatus => DaysUntilExpiry switch
        {
            <= 0 => "Expired",      // 0 и меньше - просрочено
            <= 3 => "Critical",     // 1, 2, 3 дня - критично
            <= 7 => "Warning",      // 4, 5, 6, 7 дней - внимание
            _ => "Ok"               // больше 7 - норма
        };
    }
}