namespace MyShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public double Rating { get; set; } = 0.0;
        public int ReviewCount { get; set; } = 0;

        public int CategoryId { get; set; }
        public Category CategoryNaviagation { get; set; } = null!;
        public List<ProductImage> Images { get; set; } = new List<ProductImage>();
        public string? PrimaryImageUrl => Images.FirstOrDefault(i => i.IsPrimary)?.FilePath ?? Images.FirstOrDefault()?.FilePath;
    }
}
