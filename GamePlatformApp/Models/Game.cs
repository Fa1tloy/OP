namespace GamePlatformApp.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Developer { get; set; }
        public int? ReleaseYear { get; set; }
        public decimal Price { get; set; }
        public string Power { get; set; }
        public string Photo { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}