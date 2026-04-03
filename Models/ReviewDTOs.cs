using System.ComponentModel.DataAnnotations;

namespace MyShop.Models;

// What the client sends when creating a review
public class CreateReviewDto
{
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    public string? Comment { get; set; }
}

// What we send back to the client
public class ReviewResponseDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string UserName { get; set; } = string.Empty; // Don't expose full user object
    public DateTime CreatedAt { get; set; }
}

// Summary shown on product page
public class ProductRatingSummaryDto
{
    public double AverageRating { get; set; }   // e.g. 4.3
    public int TotalReviews { get; set; }        // e.g. 127
    public Dictionary<int, int> Distribution { get; set; } = new();
    // e.g. { 5: 80, 4: 30, 3: 10, 2: 5, 1: 2 }
}