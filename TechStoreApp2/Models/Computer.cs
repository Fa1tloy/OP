namespace TechStoreApp2.Models
{
    public class Computer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specs { get; set; }        // характеристики
        public decimal PricePerHour { get; set; } // цена за час
        public string Weight { get; set; }        // вес
        public string Photo { get; set; }         // путь к фото
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}