namespace MyShop.Models;

public class Review
{
    public int Id { get; set; }

    // Which product is being reviewed
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    // Who wrote the review
    public int UserId { get; set; }
    public User? User { get; set; }

    // Rating: 1 to 5 stars
    public int Rating { get; set; }

    // Optional written review
    public string? Comment { get; set; }

    // When the review was created
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Soft flag — admin can hide inappropriate reviews
    public bool IsApproved { get; set; } = true;
}