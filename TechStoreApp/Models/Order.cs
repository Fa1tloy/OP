using System;

namespace TechStoreApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ClientName { get; set; }
        public DateTime OrderDate { get; set; }
        public int Quantity { get; set; }
        public Product Product { get; set; }   // для навигации

        // Вычисляемое поле стоимости
        public decimal Cost => (Product?.Price ?? 0) * Quantity;
    }
}