namespace TechStoreApp4.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }      // вместо Brand
        public decimal Price { get; set; }
        public string Duration { get; set; }         // вместо Power
        public string Photo { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}