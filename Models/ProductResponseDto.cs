namespace MyShop.Models;

// Returned in product listings and search results
public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public double Rating { get; set; }                       // Cached average rating
    public string? CategoryName { get; set; }                // Category name, not Id
    public string? PrimaryImage { get; set; }                // URL/path of primary image
}