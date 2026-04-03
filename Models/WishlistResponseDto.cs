namespace MyShop.Models;

// What we return when user fetches their wishlist
public class WishlistResponseDto
{
    public int Id { get; set; }          // WishlistItem Id
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; } // Primary image path
    public DateTime AddedAt { get; set; }
}