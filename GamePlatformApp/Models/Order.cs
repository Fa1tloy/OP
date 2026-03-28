using System;

namespace GamePlatformApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string ClientName { get; set; }
        public DateTime OrderDate { get; set; }
        public int Quantity { get; set; }
        public Game Game { get; set; }

        public decimal Cost => (Game?.Price ?? 0) * Quantity;
    }
}