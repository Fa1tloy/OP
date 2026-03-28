using System;

namespace TechStoreApp3.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int DishId { get; set; }
        public string ClientName { get; set; }
        public DateTime OrderDate { get; set; }
        public int Quantity { get; set; }
        public Dish Dish { get; set; }

        public decimal Cost => (Dish?.Price ?? 0) * Quantity;
    }
}