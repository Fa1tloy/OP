namespace TechStoreApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string Power { get; set; }
        public string Photo { get; set; }      // путь к файлу изображения
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}